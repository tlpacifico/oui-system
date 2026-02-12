using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class ItemPhotoConfiguration : IEntityTypeConfiguration<ItemPhotoEntity>
{
    public void Configure(EntityTypeBuilder<ItemPhotoEntity> b)
    {
        b.ToTable("ItemPhotos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.FileName).HasMaxLength(500).IsRequired();
        b.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
        b.Property(x => x.ThumbnailPath).HasMaxLength(1000);
        b.Property(x => x.DisplayOrder).IsRequired();
        b.Property(x => x.IsPrimary).HasDefaultValue(false);

        b.HasIndex(x => new { x.ItemId, x.DisplayOrder });
    }
}
