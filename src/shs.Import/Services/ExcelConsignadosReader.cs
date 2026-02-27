using ClosedXML.Excel;
using shs.Import.Models;

namespace shs.Import.Services;

/// <summary>
/// Lê o arquivo Itens Consignados_to_import.xlsx
/// Layout: sheet única "Planilha1", cabeçalho na linha 1, dados a partir da linha 2.
/// Colunas: Cliente | COD | Descriçao da peça | data de recepção | valor avaliado | situação | Crédito em dinheiro (40%) | Crédito em loja (50%)
/// </summary>
public class ExcelConsignadosReader
{
    private static readonly string[] ClienteNames = ["Cliente", "cliente", "Nome"];
    private static readonly string[] CodNames = ["COD", "Cod.", "Cod", "Código", "Codigo"];
    private static readonly string[] DescricaoNames = ["Descriçao da peça", "Descrição da peça", "Descricao da peca", "Descrição", "Descricao"];
    private static readonly string[] DataRecepcaoNames = ["data de recepção", "data de recepcao", "Data recepção", "Data de recepção"];
    private static readonly string[] ValorAvaliadoNames = ["valor avaliado", "Valor avaliado"];
    private static readonly string[] SituacaoNames = ["situação", "situacao", "Situação", "Situacao"];
    private static readonly string[] CreditoDinheiroNames = ["Crédito em dinheiro (40%)", "Credito em dinheiro (40%)", "Crédito dinheiro", "Crédito \nem dinheiro (40%)"];
    private static readonly string[] CreditoLojaNames = ["Crédito em loja (50%)", "Credito em loja (50%)", "Crédito loja"];

    public IReadOnlyList<ConsignadoRow> Read(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("Arquivo de consignados não encontrado.", filePath);

        var rows = new List<ConsignadoRow>();
        using var workbook = new XLWorkbook(filePath);

        // Nova estrutura: sheet única com coluna "Cliente"
        var sheet = workbook.Worksheet(1);
        if (sheet == null)
            throw new InvalidOperationException("Nenhuma sheet encontrada no arquivo.");

        var headerRow = sheet.Row(1);
        var colMap = BuildColumnMap(headerRow);

        if (!colMap.ContainsKey("Cliente"))
            throw new InvalidOperationException("Coluna 'Cliente' não encontrada no cabeçalho. Verifique o layout do arquivo.");

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        string? currentCliente = null;

        for (var r = 2; r <= lastRow; r++)
        {
            var row = sheet.Row(r);

            // A coluna Cliente pode estar vazia para linhas do mesmo cliente
            var clienteCell = colMap.TryGetValue("Cliente", out var clienteCol) ? GetCellString(row.Cell(clienteCol)) : null;
            if (!string.IsNullOrWhiteSpace(clienteCell))
                currentCliente = clienteCell.Trim();

            if (string.IsNullOrWhiteSpace(currentCliente))
                continue;

            var consignadoRow = MapRow(row, colMap, currentCliente);
            if (IsValidDataRow(consignadoRow))
                rows.Add(consignadoRow);
        }

        return rows;
    }

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var col = cell.Address.ColumnNumber;
            var rawValue = GetCellString(cell)?.Trim() ?? "";
            if (string.IsNullOrEmpty(rawValue)) continue;

            // Normalizar quebras de linha no cabeçalho (ex: "Crédito \nem dinheiro (40%)")
            var value = rawValue.Replace("\n", " ").Replace("\r", " ").Trim();
            while (value.Contains("  "))
                value = value.Replace("  ", " ");

            if (ClienteNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["Cliente"] = col;
            else if (CodNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["COD"] = col;
            else if (DescricaoNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["Descrição"] = col;
            else if (DataRecepcaoNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["data de recepção"] = col;
            else if (ValorAvaliadoNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["valor avaliado"] = col;
            else if (SituacaoNames.Contains(value, StringComparer.OrdinalIgnoreCase)) map["situação"] = col;
            else if (CreditoDinheiroNames.Contains(value, StringComparer.OrdinalIgnoreCase)
                     || value.Contains("dinheiro", StringComparison.OrdinalIgnoreCase)) map["Crédito dinheiro"] = col;
            else if (CreditoLojaNames.Contains(value, StringComparer.OrdinalIgnoreCase)
                     || value.Contains("loja", StringComparison.OrdinalIgnoreCase)) map["Crédito loja"] = col;
        }
        return map;
    }

    private static ConsignadoRow MapRow(IXLRow row, Dictionary<string, int> colMap, string supplierName)
    {
        string? Get(string key) => colMap.TryGetValue(key, out var c) ? GetCellString(row.Cell(c)) : null;

        return new ConsignadoRow
        {
            SupplierName = supplierName,
            Cod = Get("COD"),
            Descricao = Get("Descrição"),
            DataRecepcao = Get("data de recepção"),
            ValorAvaliado = Get("valor avaliado"),
            Situacao = Get("situação"),
            CreditoDinheiro = Get("Crédito dinheiro"),
            CreditoLoja = Get("Crédito loja")
        };
    }

    private static bool IsValidDataRow(ConsignadoRow r)
    {
        if (string.IsNullOrWhiteSpace(r.Descricao) && string.IsNullOrWhiteSpace(r.Cod))
            return false;
        var desc = (r.Descricao ?? "").Trim();
        if (desc.Equals("Total", StringComparison.OrdinalIgnoreCase) ||
            desc.StartsWith("Total ", StringComparison.OrdinalIgnoreCase))
            return false;
        return true;
    }

    private static string? GetCellString(IXLCell cell)
    {
        var v = cell.Value;
        if (v.IsBlank) return null;
        if (v.IsDateTime)
            return v.GetDateTime().ToString("yyyy-MM-dd");
        return v.ToString()?.Trim();
    }
}
