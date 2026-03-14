using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Infrastructure.Configurations;

internal class SaleItemConfiguration : IEntityTypeConfiguration<SaleItemEntity>
{
    public void Configure(EntityTypeBuilder<SaleItemEntity> b)
    {
        b.ToTable("SaleItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalPrice).HasPrecision(18, 2);

        // Cross-module FK to Inventory — no navigation, just FK property + index
        b.Property(x => x.ItemId).IsRequired();
        b.HasIndex(x => x.ItemId);

        b.HasOne(x => x.Settlement)
            .WithMany(s => s.SaleItems)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SettlementId);
    }
}
