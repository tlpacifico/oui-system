using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class BrandEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
