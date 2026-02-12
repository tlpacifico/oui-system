namespace shs.Domain.Entities;

public class SaleItemEntity
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public long ItemId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalPrice { get; set; }

    public SaleEntity Sale { get; set; } = null!;
    public ItemEntity Item { get; set; } = null!;
}
