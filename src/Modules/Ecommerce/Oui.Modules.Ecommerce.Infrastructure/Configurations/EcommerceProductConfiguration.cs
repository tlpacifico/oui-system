using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Ecommerce.Infrastructure.Configurations;

internal class EcommerceProductConfiguration : IEntityTypeConfiguration<EcommerceProductEntity>
{
    public void Configure(EntityTypeBuilder<EcommerceProductEntity> b)
    {
        b.ToTable("EcommerceProducts");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();

        b.Property(x => x.Slug).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();

        b.Property(x => x.Title).HasMaxLength(500).IsRequired();
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.Price).HasPrecision(18, 2);
        b.Property(x => x.BrandName).HasMaxLength(200).IsRequired();
        b.Property(x => x.CategoryName).HasMaxLength(200);
        b.Property(x => x.Size).HasMaxLength(20);
        b.Property(x => x.Color).HasMaxLength(100);
        b.Property(x => x.Condition).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Composition).HasMaxLength(500);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

        // Indexes for public queries
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.BrandName);
        b.HasIndex(x => x.CategoryName);
        b.HasIndex(x => x.Price);

        // Cross-module FK to Inventory (Item) — no navigation, just FK property + index
        b.Property(x => x.ItemId).IsRequired();
        b.HasIndex(x => x.ItemId);

        // Soft delete
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        // Intra-module relationships
        b.HasMany(x => x.Photos)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
