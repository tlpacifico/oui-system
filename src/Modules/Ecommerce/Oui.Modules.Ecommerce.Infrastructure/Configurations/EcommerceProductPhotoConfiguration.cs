using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Ecommerce.Infrastructure.Configurations;

internal class EcommerceProductPhotoConfiguration : IEntityTypeConfiguration<EcommerceProductPhotoEntity>
{
    public void Configure(EntityTypeBuilder<EcommerceProductPhotoEntity> b)
    {
        b.ToTable("EcommerceProductPhotos");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.FilePath).HasMaxLength(1000).IsRequired();
        b.Property(x => x.ThumbnailPath).HasMaxLength(1000);
        b.Property(x => x.DisplayOrder).IsRequired();
        b.Property(x => x.IsPrimary).HasDefaultValue(false);
        b.HasIndex(x => new { x.ProductId, x.DisplayOrder });
    }
}
