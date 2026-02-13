using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItemEntity>
{
    public void Configure(EntityTypeBuilder<SaleItemEntity> b)
    {
        b.ToTable("SaleItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.FinalPrice).HasPrecision(18, 2);
        b.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Settlement).WithMany(s => s.SaleItems).HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.SettlementId);
    }
}
