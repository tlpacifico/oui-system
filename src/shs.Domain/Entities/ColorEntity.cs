using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class ColorEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string? HexCode { get; set; } // Hex color for UI swatch (e.g. #1C1917)

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation - Many-to-Many with Items
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
