namespace shs.Domain.Entities;

public class EcommerceOrderItemEntity : EntityWithIdAuditable<long>
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long ItemId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Navigation (intra-module: Ecommerce)
    public EcommerceOrderEntity Order { get; set; } = null!;
    public EcommerceProductEntity Product { get; set; } = null!;
}
