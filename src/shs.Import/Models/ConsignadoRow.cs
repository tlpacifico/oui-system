namespace shs.Import.Models;

/// <summary>
/// DTO para uma linha do arquivo Itens Consignados_to_import.xlsx
/// Sheet única com coluna "Cliente" identificando o fornecedor.
/// </summary>
public record ConsignadoRow
{
    public string SupplierName { get; init; } = string.Empty;
    public string? Cod { get; init; }
    public string? Descricao { get; init; }
    public string? DataRecepcao { get; init; }
    public string? ValorAvaliado { get; init; }
    public string? Situacao { get; init; }
    public string? CreditoDinheiro { get; init; }
    public string? CreditoLoja { get; init; }
}
