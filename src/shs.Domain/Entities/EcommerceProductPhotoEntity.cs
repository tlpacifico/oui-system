namespace shs.Domain.Entities;

public class EcommerceProductPhotoEntity : EntityWithIdAuditable<long>
{
    public long ProductId { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? ThumbnailPath { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsPrimary { get; set; }

    // Navigation
    public EcommerceProductEntity Product { get; set; } = null!;
}
