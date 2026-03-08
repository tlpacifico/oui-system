namespace shs.Infrastructure.Services.Import.Models;

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
