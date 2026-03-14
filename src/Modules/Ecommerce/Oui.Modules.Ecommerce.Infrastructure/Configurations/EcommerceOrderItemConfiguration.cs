using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Ecommerce.Infrastructure.Configurations;

internal class EcommerceOrderItemConfiguration : IEntityTypeConfiguration<EcommerceOrderItemEntity>
{
    public void Configure(EntityTypeBuilder<EcommerceOrderItemEntity> b)
    {
        b.ToTable("EcommerceOrderItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();

        b.Property(x => x.ProductTitle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Price).HasPrecision(18, 2);

        b.HasIndex(x => x.ProductId);

        // Cross-module FK to Inventory (Item) — no navigation, just FK property + index
        b.Property(x => x.ItemId).IsRequired();
        b.HasIndex(x => x.ItemId);

        // Intra-module relationship
        b.HasOne(x => x.Product)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
