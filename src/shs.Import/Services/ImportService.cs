using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Import.Models;
using shs.Infrastructure.Database;

namespace shs.Import.Services;

/// <summary>
/// Serviço de importação de dados dos arquivos Excel para a base de dados.
/// Ordem: Brands → Suppliers → Receptions (opcional) → Items
/// </summary>
public class ImportService
{
    private const string DefaultBrandName = "Geral";
    private const int BatchSize = 100;

    // Origens que indicam compra própria (não consignação)
    private static readonly string[] OwnPurchaseOrigins =
    [
        "Ac. pessoal", "Ac. Pessoal", "Ac pessoal", "Ac.pessoal",
        "Acp", "Acp Lilly", "acp", "ac. pessoal", "AC", "ACP"
    ];

    // Origens que mapeiam para ItemOrigin específico
    private static readonly string[] HumanaOrigins = ["Humana", "humana"];
    private static readonly string[] VintedOrigins = ["Vinted", "vinted", "VT"];
    private static readonly string[] HmOrigins = ["HM", "H&M"];
    private static readonly string[] DonationOrigins = ["Doação", "doação", "Doa", "doa"];
    private static readonly string[] StorePurchaseOrigins = ["compra em loja"];

    private readonly ShsDbContext _db;
    private readonly ILogger<ImportService> _logger;
    private readonly ExcelEstoqueReader _estoqueReader;
    private readonly ExcelConsignadosReader _consignadosReader;

    private Dictionary<string, long> _brandCache = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, long> _supplierCache = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<(long SupplierId, DateTime Date), long> _receptionCache = new();
    private HashSet<string> _existingIdentificationNumbers = new(StringComparer.OrdinalIgnoreCase);
    private HashSet<string> _usedSupplierInitials = new(StringComparer.OrdinalIgnoreCase);

    public ImportService(
        ShsDbContext db,
        ILogger<ImportService> logger,
        ExcelEstoqueReader estoqueReader,
        ExcelConsignadosReader consignadosReader)
    {
        _db = db;
        _logger = logger;
        _estoqueReader = estoqueReader;
        _consignadosReader = consignadosReader;
    }

    public async Task<ImportResult> ImportAsync(string estoquePath, string consignadosPath, CancellationToken ct = default)
    {
        var result = new ImportResult();
        _brandCache.Clear();
        _supplierCache.Clear();
        _receptionCache.Clear();
        _existingIdentificationNumbers = (await _db.Items.IgnoreQueryFilters()
            .Select(x => x.IdentificationNumber)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        _usedSupplierInitials = (await _db.Suppliers.IgnoreQueryFilters()
            .Select(x => x.Initial)
            .ToListAsync(ct))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        try
        {
            // 1. Ler dados dos Excel
            _logger.LogInformation("Lendo arquivo de itens pessoais: {Path}", estoquePath);
            var estoqueRows = _estoqueReader.Read(estoquePath);
            result.EstoqueRowsRead = estoqueRows.Count;

            _logger.LogInformation("Lendo arquivo de consignados: {Path}", consignadosPath);
            var consignadoRows = _consignadosReader.Read(consignadosPath);
            result.ConsignadoRowsRead = consignadoRows.Count;

            // 2. Importar Marcas (do estoque)
            var marcas = estoqueRows
                .Select(r => NormalizeBrand(r.Marca))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            marcas.Add(DefaultBrandName);
            foreach (var nome in marcas.Distinct())
            {
                var created = await EnsureBrandAsync(nome, ct);
                if (created) result.BrandsCreated++;
            }

            // 3. Importar Fornecedores (dos consignados + origens consignação do estoque)
            var supplierNamesFromConsignados = consignadoRows
                .Select(r => r.SupplierName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var name in supplierNamesFromConsignados)
            {
                var created = await EnsureSupplierAsync(name, ct);
                if (created) result.SuppliersCreated++;
            }

            var supplierNamesFromEstoque = estoqueRows
                .Select(r => r.Origem)
                .Where(x => !string.IsNullOrWhiteSpace(x) && IsConsignmentOrigin(x!))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var name in supplierNamesFromEstoque)
            {
                var created = await EnsureSupplierAsync(name!, ct);
                if (created) result.SuppliersCreated++;
            }

            // 4. Importar Itens do Estoque (itens pessoais)
            foreach (var row in estoqueRows)
            {
                try
                {
                    var (created, skipped) = await ImportEstoqueItemAsync(row, ct);
                    if (created) result.ItemsFromEstoque++;
                    else if (skipped) result.Errors++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro ao importar linha estoque: Ref={Ref}", row.RefPeca);
                    result.Errors++;
                }
            }
            await _db.SaveChangesAsync(ct);

            // 5. Importar Itens dos Consignados
            var consignadoRowsBySupplier = consignadoRows.GroupBy(r => r.SupplierName, StringComparer.OrdinalIgnoreCase);
            foreach (var group in consignadoRowsBySupplier)
            {
                var supplierId = await GetSupplierIdAsync(group.Key, ct);
                if (supplierId == null)
                {
                    _logger.LogWarning("Fornecedor não encontrado: {Name}", group.Key);
                    continue;
                }

                var rowIndex = 0;
                foreach (var row in group)
                {
                    try
                    {
                        var (created, skipped) = await ImportConsignadoItemAsync(row, supplierId.Value, rowIndex++, ct);
                        if (created) result.ItemsFromConsignados++;
                        else if (skipped) result.Errors++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Erro ao importar linha consignado: Supplier={Supplier}, Cod={Cod}",
                            row.SupplierName, row.Cod);
                        result.Errors++;
                    }
                }
            }
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Importação concluída. Marcas: {Brands}, Fornecedores: {Suppliers}, " +
                "Itens Pessoais: {Estoque}, Itens Consignados: {Consignados}, Erros: {Erros}",
                result.BrandsCreated, result.SuppliersCreated, result.ItemsFromEstoque,
                result.ItemsFromConsignados, result.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha na importação");
            throw;
        }

        return result;
    }

    private static string NormalizeBrand(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        return value.Trim();
    }

    private static bool IsOwnPurchaseOrigin(string origem)
    {
        var o = origem.Trim();
        return OwnPurchaseOrigins.Contains(o, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsNonSupplierOrigin(string origem)
    {
        var o = origem.Trim();
        return IsOwnPurchaseOrigin(origem)
            || HumanaOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || VintedOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || HmOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || DonationOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || StorePurchaseOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || o.Equals("Brasil", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsConsignmentOrigin(string origem)
    {
        return !IsOwnPurchaseOrigin(origem) && !IsNonSupplierOrigin(origem);
    }

    private static ItemOrigin MapItemOrigin(string? origem)
    {
        if (string.IsNullOrWhiteSpace(origem)) return ItemOrigin.PersonalCollection;
        var o = origem.Trim();

        if (OwnPurchaseOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
            return ItemOrigin.PersonalCollection;
        if (HumanaOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
            return ItemOrigin.Humana;
        if (VintedOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
            return ItemOrigin.Vinted;
        if (HmOrigins.Contains(o, StringComparer.OrdinalIgnoreCase))
            return ItemOrigin.HM;
        if (DonationOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || StorePurchaseOrigins.Contains(o, StringComparer.OrdinalIgnoreCase)
            || o.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
            return ItemOrigin.Other;

        // Nomes de pessoas = consignação (ex: "C/ Gi", "Carol", "Cris", "Rita", etc.)
        return ItemOrigin.Consignment;
    }

    private async Task<bool> EnsureBrandAsync(string name, CancellationToken ct)
    {
        var key = name.Trim();
        if (_brandCache.TryGetValue(key, out var id))
            return false;

        var existing = await _db.Brands.IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.Name.ToLower() == key.ToLower(), ct);
        if (existing != null)
        {
            _brandCache[key] = existing.Id;
            return false;
        }

        var brand = new BrandEntity
        {
            Name = key,
            ExternalId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };
        _db.Brands.Add(brand);
        await _db.SaveChangesAsync(ct);
        _brandCache[key] = brand.Id;
        return true;
    }

    private async Task<bool> EnsureSupplierAsync(string name, CancellationToken ct)
    {
        var key = name.Trim();
        if (string.IsNullOrEmpty(key)) return false;
        if (_supplierCache.TryGetValue(key, out var id))
            return false;

        var existing = await _db.Suppliers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == key.ToLower(), ct);
        if (existing != null)
        {
            _supplierCache[key] = existing.Id;
            return false;
        }

        var initial = GenerateUniqueInitial(key);
        var supplier = new SupplierEntity
        {
            Name = key,
            Initial = initial,
            Email = $"{initial.ToLowerInvariant()}@consignado.placeholder",
            PhoneNumber = ExtractPhoneFromName(name) ?? "+351000000000",
            ExternalId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };
        _db.Suppliers.Add(supplier);
        await _db.SaveChangesAsync(ct);
        _supplierCache[key] = supplier.Id;
        _usedSupplierInitials.Add(initial);
        return true;
    }

    private static string? ExtractPhoneFromName(string name)
    {
        // Alguns nomes incluem telefone: "Carina Mocho(910 222 714)"
        var match = System.Text.RegularExpressions.Regex.Match(name, @"\(?\s*(\d{3})\s*(\d{3})\s*(\d{3})\s*\)?");
        if (match.Success)
            return $"+351{match.Groups[1].Value}{match.Groups[2].Value}{match.Groups[3].Value}";
        return null;
    }

    private string GenerateUniqueInitial(string name)
    {
        // Remover telefone do nome para gerar iniciais
        var cleanName = System.Text.RegularExpressions.Regex.Replace(name, @"\(.*?\)", "").Trim();
        var words = cleanName.Split([' ', '(', ')', '-'], StringSplitOptions.RemoveEmptyEntries);
        var initial = words.Length >= 2
            ? $"{char.ToUpperInvariant(words[0][0])}{char.ToUpperInvariant(words[1][0])}"
            : cleanName.Length >= 2
                ? cleanName[..2].ToUpperInvariant()
                : cleanName.ToUpperInvariant();

        initial = new string(initial.Where(char.IsLetter).ToArray());
        if (string.IsNullOrEmpty(initial)) initial = "X";

        var baseInitial = initial;
        var suffix = 1;
        while (_usedSupplierInitials.Contains(initial))
        {
            initial = baseInitial.Length <= 3 ? $"{baseInitial}{suffix}" : $"{baseInitial[..3]}{suffix}";
            suffix++;
        }
        return initial;
    }

    private async Task<long?> GetSupplierIdAsync(string name, CancellationToken ct)
    {
        var key = name.Trim();
        if (_supplierCache.TryGetValue(key, out var id))
            return id;
        var existing = await _db.Suppliers.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Name.ToLower() == key.ToLower(), ct);
        if (existing != null)
        {
            _supplierCache[key] = existing.Id;
            return existing.Id;
        }
        return null;
    }

    private async Task<long> GetOrCreateReceptionAsync(long supplierId, DateTime date, CancellationToken ct)
    {
        var key = (supplierId, date.Date);
        if (_receptionCache.TryGetValue(key, out var id))
            return id;

        var existing = await _db.Receptions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.SupplierId == supplierId && r.ReceptionDate.Date == date.Date, ct);
        if (existing != null)
        {
            _receptionCache[key] = existing.Id;
            return existing.Id;
        }

        var reception = new ReceptionEntity
        {
            SupplierId = supplierId,
            ReceptionDate = date,
            ItemCount = 0,
            Status = ReceptionStatus.Evaluated,
            ExternalId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };
        _db.Receptions.Add(reception);
        await _db.SaveChangesAsync(ct);
        _receptionCache[key] = reception.Id;
        return reception.Id;
    }

    private async Task<(bool Created, bool Skipped)> ImportEstoqueItemAsync(EstoqueRow row, CancellationToken ct)
    {
        var brandName = string.IsNullOrWhiteSpace(row.Marca) ? DefaultBrandName : NormalizeBrand(row.Marca);
        if (!_brandCache.TryGetValue(brandName, out var brandId))
        {
            _logger.LogWarning("Marca não encontrada: {Marca}", brandName);
            return (false, true);
        }

        var identificationNumber = EnsureUniqueIdentification(row.RefPeca ?? $"IMP-{Guid.NewGuid():N}"[..12]);
        var (acquisitionType, supplierId) = await ResolveOriginAsync(row.Origem, ct);
        var itemOrigin = MapItemOrigin(row.Origem);
        long? receptionId = null;
        DateTime? receptionDate = null;

        if (DateTime.TryParse(row.DataAquisicao, out var dt))
            receptionDate = dt;

        if (supplierId != null && receptionDate != null)
        {
            receptionId = await GetOrCreateReceptionAsync(supplierId.Value, receptionDate.Value, ct);
        }

        var item = new ItemEntity
        {
            IdentificationNumber = identificationNumber,
            Name = (row.Descricao ?? "Sem descrição").Trim().Truncate(500) ?? "Sem descrição",
            BrandId = brandId,
            Size = StringExtensions.TruncateOrDefault(row.Tam, 20, "—") ?? "—",
            Color = StringExtensions.TruncateOrDefault(row.Cor, 100, "—") ?? "—",
            Composition = row.Composicao?.Trim().Truncate(500),
            Condition = MapCondition(row.Condicao),
            EvaluatedPrice = ParseDecimal(row.ValorSugerido ?? row.ValorVenda) ?? 0,
            CostPrice = ParseDecimal(row.ValorCompra),
            FinalSalePrice = ParseDecimal(row.ValorVenda),
            Status = MapSituacaoEstoque(row.Situacao),
            AcquisitionType = acquisitionType,
            Origin = itemOrigin,
            SupplierId = supplierId,
            ReceptionId = receptionId,
            CommissionPercentage = acquisitionType == AcquisitionType.Consignment ? 50m : 0,
            ExternalId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        _db.Items.Add(item);
        _existingIdentificationNumbers.Add(identificationNumber);
        return (true, false);
    }

    private async Task<(bool Created, bool Skipped)> ImportConsignadoItemAsync(ConsignadoRow row, long supplierId, int rowIndex, CancellationToken ct)
    {
        var fallbackCod = string.IsNullOrWhiteSpace(row.Cod)
            ? $"{GetInitialsFromName(row.SupplierName)}{rowIndex:D4}"
            : row.Cod;
        var identificationNumber = EnsureUniqueIdentification(fallbackCod);
        var brandId = _brandCache.TryGetValue(DefaultBrandName, out var b) ? b : _brandCache.Values.First();

        long? receptionId = null;
        if (DateTime.TryParse(row.DataRecepcao, out var dt))
            receptionId = await GetOrCreateReceptionAsync(supplierId, dt, ct);

        var item = new ItemEntity
        {
            IdentificationNumber = identificationNumber,
            Name = (row.Descricao ?? "Sem descrição").Trim().Truncate(500) ?? "Sem descrição",
            BrandId = brandId,
            Size = "—",
            Color = "—",
            Condition = ItemCondition.Good,
            EvaluatedPrice = ParseDecimal(row.ValorAvaliado) ?? 0,
            Status = MapSituacaoConsignados(row.Situacao),
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = supplierId,
            ReceptionId = receptionId,
            CommissionPercentage = 50m,
            ExternalId = Guid.NewGuid(),
            CreatedOn = DateTime.UtcNow
        };

        _db.Items.Add(item);
        _existingIdentificationNumbers.Add(identificationNumber);
        return (true, false);
    }

    private string EnsureUniqueIdentification(string baseId)
    {
        var id = (baseId ?? "").Trim();
        if (string.IsNullOrEmpty(id)) id = $"IMP-{Guid.NewGuid():N}"[..12];
        if (id.Length > 32) id = id[..32];

        if (!_existingIdentificationNumbers.Contains(id))
            return id;

        var suffix = 2;
        while (_existingIdentificationNumbers.Contains($"{id}-{suffix}"))
            suffix++;
        return $"{id}-{suffix}";
    }

    private async Task<(AcquisitionType Type, long? SupplierId)> ResolveOriginAsync(string? origem, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(origem))
            return (AcquisitionType.OwnPurchase, null);

        var o = origem.Trim();

        // Compra própria: sem fornecedor
        if (IsOwnPurchaseOrigin(o))
            return (AcquisitionType.OwnPurchase, null);

        // Origens externas sem fornecedor (Humana, Vinted, HM, Doação, etc.)
        if (IsNonSupplierOrigin(o))
            return (AcquisitionType.OwnPurchase, null);

        // Consignação: nome de pessoa = fornecedor
        // Nomes como "C/ Gi" → procurar "Gi", "Carol", "Cris", etc.
        var supplierName = NormalizeSupplierOriginName(o);
        var supplierId = await GetSupplierIdAsync(supplierName, ct);
        if (supplierId == null)
        {
            // Tentar com o nome original
            supplierId = await GetSupplierIdAsync(o, ct);
        }
        return (AcquisitionType.Consignment, supplierId);
    }

    private static string NormalizeSupplierOriginName(string origem)
    {
        var o = origem.Trim();
        // "C/ Gi" → "Gi", "c/ Gi" → "Gi"
        if (o.StartsWith("C/ ", StringComparison.OrdinalIgnoreCase) ||
            o.StartsWith("c/ ", StringComparison.OrdinalIgnoreCase))
            return o[3..].Trim();
        return o;
    }

    /// <summary>
    /// Mapeia situação do arquivo de itens pessoais (Personal_items_to_import.xlsx).
    /// Valores encontrados: CS, DL, DV, Devolvido, Disponivel, Disponível, VD, Vendida, Vendido, Vendido_VAL, Vendido/Cris, Vendido/Gi, Gisele, dispon
    /// </summary>
    private static ItemStatus MapSituacaoEstoque(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ItemStatus.ToSell;
        var v = value.Trim().ToUpperInvariant();

        if (v is "CS") return ItemStatus.Evaluated;
        if (v is "DL" || v.StartsWith("DISPONIVEL") || v.StartsWith("DISPONÍVEL") || v == "DISPONIVEL" || v == "DISPONÍVEL")
            return ItemStatus.ToSell;
        if (v is "VD" || v.StartsWith("VENDID") || v.StartsWith("VENDIDA"))
            return ItemStatus.Sold;
        if (v is "DV" || v.StartsWith("DEVOLVID"))
            return ItemStatus.Returned;

        // "Gisele" e outros nomes usados como situação → provavelmente consignado/vendido a alguém
        _= v; // fallback
        return ItemStatus.ToSell;
    }

    /// <summary>
    /// Mapeia situação do arquivo de consignados (Itens Consignados_to_import.xlsx).
    /// Valores: DL, VD, VENDIDO, DV, Devolvido, Devolver, PG, Pago, CS, AV, CD, DOAÇÃO, defeito, com defeito,
    ///          Disponível em loja, crédito resgatado, e variantes com datas (PG 28.02, DV em 10.07)
    /// </summary>
    private static ItemStatus MapSituacaoConsignados(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ItemStatus.ToSell;
        var v = value.Trim().ToUpperInvariant();

        // Disponível / à venda
        if (v is "DL" || v.StartsWith("DISPONÍVEL") || v.StartsWith("DISPONIVEL"))
            return ItemStatus.ToSell;

        // Vendido
        if (v is "VD" || v.StartsWith("VENDID") || v == "VD ")
            return ItemStatus.Sold;

        // Devolvido (incluindo variantes com data: "DV em 10.07", "Devolvido em 03.10", "Devolver")
        if (v is "DV" || v.StartsWith("DV ") || v.StartsWith("DEVOLVID") || v.StartsWith("DEVOLVER"))
            return ItemStatus.Returned;

        // Pago (incluindo "PG 28.02", "PG em 06.03", "Pago", "Pago em 10.07")
        if (v is "PG" || v.StartsWith("PG ") || v.StartsWith("PAGO"))
            return ItemStatus.Paid;

        // Avaliado
        if (v is "AV") return ItemStatus.Evaluated;

        // Em consignação / aguardando
        if (v is "CS") return ItemStatus.Evaluated;

        // Crédito já resgatado → considerar como pago
        if (v is "CD" || v.Contains("CRÉDITO") || v.Contains("CREDITO") || v.Contains("RESGATADO"))
            return ItemStatus.Paid;

        // Doação
        if (v.Contains("DOAÇ") || v.Contains("DOACAO"))
            return ItemStatus.Returned;

        // Defeito → item com problema, tratar como devolvido
        if (v.Contains("DEFEITO"))
            return ItemStatus.Returned;

        // Números avulsos (ex: "6", "8", "12", "13") → provavelmente valores, não status → default
        return ItemStatus.ToSell;
    }

    private static ItemCondition MapCondition(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return ItemCondition.Good;
        var v = value.Trim().ToUpperInvariant();
        if (v is "EXCELENTE" or "EX") return ItemCondition.Excellent;
        if (v is "MUITO BOM" or "MB") return ItemCondition.VeryGood;
        if (v is "BOM" or "B" or "BOM ESTADO") return ItemCondition.Good;
        if (v is "RAZOAVEL" or "RAZOÁVEL") return ItemCondition.Fair;
        if (v is "POBRE") return ItemCondition.Poor;
        return ItemCondition.Good;
    }

    private static string GetInitialsFromName(string name)
    {
        var cleanName = System.Text.RegularExpressions.Regex.Replace(name, @"\(.*?\)", "").Trim();
        var words = cleanName.Split([' ', '(', ')', '-'], StringSplitOptions.RemoveEmptyEntries);
        if (words.Length >= 2)
            return $"{char.ToUpperInvariant(words[0][0])}{char.ToUpperInvariant(words[1][0])}";
        if (cleanName.Length >= 2)
            return cleanName[..2].ToUpperInvariant();
        return "C";
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Replace(",", ".");
        if (decimal.TryParse(value, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var d))
            return d;
        return null;
    }
}

public class ImportResult
{
    public int EstoqueRowsRead { get; set; }
    public int ConsignadoRowsRead { get; set; }
    public int BrandsCreated { get; set; }
    public int SuppliersCreated { get; set; }
    public int ItemsFromEstoque { get; set; }
    public int ItemsFromConsignados { get; set; }
    public int Errors { get; set; }
}

file static class StringExtensions
{
    public static string? Truncate(this string? value, int maxLength)
    {
        if (value == null) return null;
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    public static string? TruncateOrDefault(string? value, int maxLength, string _)
    {
        var s = (value ?? "").Trim();
        if (string.IsNullOrEmpty(s)) return null;
        return s.Length <= maxLength ? s : s[..maxLength];
    }
}
