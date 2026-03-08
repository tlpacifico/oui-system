using ClosedXML.Excel;
using shs.Infrastructure.Services.Import.Models;

namespace shs.Infrastructure.Services.Import;

public class ExcelEstoqueReader
{
    private static readonly string[] RefPecaNames = ["Ref Peça", "Ref Peca", "RefPeça", "RefPeca"];
    private static readonly string[] LocalizacaoNames = ["Localização", "Localizacao"];
    private static readonly string[] OrigemNames = ["Origem"];
    private static readonly string[] SituacaoNames = ["Situação", "Situacao"];
    private static readonly string[] DescricaoNames = ["Descrição", "Descricao"];
    private static readonly string[] MarcaNames = ["Marca"];
    private static readonly string[] CondicaoNames = ["Condição", "Condicao"];
    private static readonly string[] TamNames = ["Tam", "Tamanho"];
    private static readonly string[] CorNames = ["Cor"];
    private static readonly string[] ComposicaoNames = ["Composição", "Composicao"];
    private static readonly string[] DataAquisicaoNames = ["Data da aquisição", "Data da aquisicao", "Data aquisição"];
    private static readonly string[] PublicadoEmNames = ["Publicado em:", "Publicado em", "Publicado"];
    private static readonly string[] ValorReferenciaNames = ["Valor de referência", "Valor de referencia"];
    private static readonly string[] ValorCompraNames = ["Valor de Compra", "Valor Compra"];
    private static readonly string[] ValorSugeridoNames = ["Valor sugerido"];
    private static readonly string[] ValorVendaNames = ["Valor venda"];

    private static readonly string[][] AllNameArrays =
    [
        RefPecaNames, LocalizacaoNames, OrigemNames, SituacaoNames, DescricaoNames,
        MarcaNames, CondicaoNames, TamNames, CorNames, ComposicaoNames,
        DataAquisicaoNames, PublicadoEmNames, ValorReferenciaNames, ValorCompraNames,
        ValorSugeridoNames, ValorVendaNames
    ];

    public IReadOnlyList<EstoqueRow> Read(Stream stream)
    {
        var rows = new List<EstoqueRow>();
        using var workbook = new XLWorkbook(stream);

        var sheet = workbook.TryGetWorksheet("Página1", out var ws) ? ws : workbook.Worksheet(1);
        if (sheet == null)
            throw new InvalidOperationException("Sheet 'Página1' não encontrado.");

        var headerRow = sheet.Row(1);
        var colMap = BuildColumnMap(headerRow);

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var r = 2; r <= lastRow; r++)
        {
            var row = sheet.Row(r);
            var estoqueRow = MapRow(row, colMap);
            if (IsValidDataRow(estoqueRow))
                rows.Add(estoqueRow);
        }

        return rows;
    }

    private static Dictionary<string, int> BuildColumnMap(IXLRow headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in headerRow.CellsUsed())
        {
            var col = cell.Address.ColumnNumber;
            var value = GetCellString(cell)?.Trim() ?? "";
            if (string.IsNullOrEmpty(value)) continue;

            foreach (var names in AllNameArrays)
                TryAdd(map, value, names, col);
        }
        return map;
    }

    private static void TryAdd(Dictionary<string, int> map, string value, string[] names, int col)
    {
        if (names.Contains(value, StringComparer.OrdinalIgnoreCase))
            map[names[0]] = col;
    }

    private static EstoqueRow MapRow(IXLRow row, Dictionary<string, int> colMap)
    {
        string? Get(string key) => colMap.TryGetValue(key, out var c) ? GetCellString(row.Cell(c)) : null;

        return new EstoqueRow
        {
            RefPeca = Get(RefPecaNames[0]),
            Localizacao = Get(LocalizacaoNames[0]),
            Origem = Get(OrigemNames[0]),
            Situacao = Get(SituacaoNames[0]),
            Descricao = Get(DescricaoNames[0]),
            Marca = Get(MarcaNames[0]),
            Condicao = Get(CondicaoNames[0]),
            Tam = Get(TamNames[0]),
            Cor = Get(CorNames[0]),
            Composicao = Get(ComposicaoNames[0]),
            DataAquisicao = Get(DataAquisicaoNames[0]),
            PublicadoEm = Get(PublicadoEmNames[0]),
            ValorReferencia = Get(ValorReferenciaNames[0]),
            ValorCompra = Get(ValorCompraNames[0]),
            ValorSugerido = Get(ValorSugeridoNames[0]),
            ValorVenda = Get(ValorVendaNames[0])
        };
    }

    private static bool IsValidDataRow(EstoqueRow r)
    {
        if (string.IsNullOrWhiteSpace(r.Descricao) && string.IsNullOrWhiteSpace(r.RefPeca))
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
