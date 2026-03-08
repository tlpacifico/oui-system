namespace shs.Infrastructure.Services.Import;

public class ImportResult
{
    public int EstoqueRowsRead { get; set; }
    public int ConsignadoRowsRead { get; set; }
    public int BrandsCreated { get; set; }
    public int SuppliersCreated { get; set; }
    public int ItemsFromEstoque { get; set; }
    public int ItemsFromConsignados { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorDetails { get; set; } = [];
}
