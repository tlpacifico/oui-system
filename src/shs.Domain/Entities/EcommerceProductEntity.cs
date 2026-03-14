using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class EcommerceProductEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public long ItemId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Size { get; set; }
    public string? Color { get; set; }
    public ItemCondition Condition { get; set; }
    public string? Composition { get; set; }
    public EcommerceProductStatus Status { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? UnpublishedAt { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation properties (intra-module: Ecommerce)
    public ICollection<EcommerceProductPhotoEntity> Photos { get; set; } = new List<EcommerceProductPhotoEntity>();
    public ICollection<EcommerceOrderItemEntity> OrderItems { get; set; } = new List<EcommerceOrderItemEntity>();
}
