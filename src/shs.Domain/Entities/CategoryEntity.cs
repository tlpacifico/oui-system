using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class CategoryEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public long? ParentCategoryId { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation
    public CategoryEntity? ParentCategory { get; set; }
    public ICollection<CategoryEntity> SubCategories { get; set; } = new List<CategoryEntity>();
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
