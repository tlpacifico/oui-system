namespace shs.Domain.Entities;

public class ItemPhotoEntity : EntityWithIdAuditable<long>
{
    public long ItemId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty; // Relative path
    public string? ThumbnailPath { get; set; }
    public int DisplayOrder { get; set; } // Order for display (1, 2, 3...)
    public bool IsPrimary { get; set; } // First photo is primary

    // Navigation
    public ItemEntity Item { get; set; } = null!;
}
