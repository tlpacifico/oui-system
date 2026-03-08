namespace shs.Infrastructure.Services.Import.Models;

public record EstoqueRow
{
    public string? RefPeca { get; init; }
    public string? Localizacao { get; init; }
    public string? Origem { get; init; }
    public string? Situacao { get; init; }
    public string? Descricao { get; init; }
    public string? Marca { get; init; }
    public string? Condicao { get; init; }
    public string? Tam { get; init; }
    public string? Cor { get; init; }
    public string? Composicao { get; init; }
    public string? DataAquisicao { get; init; }
    public string? PublicadoEm { get; init; }
    public string? ValorReferencia { get; init; }
    public string? ValorCompra { get; init; }
    public string? ValorSugerido { get; init; }
    public string? ValorVenda { get; init; }
}
