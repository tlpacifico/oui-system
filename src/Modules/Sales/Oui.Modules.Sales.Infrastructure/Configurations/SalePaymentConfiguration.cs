using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Infrastructure.Configurations;

internal class SalePaymentConfiguration : IEntityTypeConfiguration<SalePaymentEntity>
{
    public void Configure(EntityTypeBuilder<SalePaymentEntity> b)
    {
        b.ToTable("SalePayments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Reference).HasMaxLength(256);
        b.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(32);

        // Cross-module FK to Inventory (Supplier) — no navigation, just FK property + index
        b.Property(x => x.SupplierId);
        b.HasIndex(x => x.SupplierId);

        // Intra-module relationship
        b.HasOne(x => x.StoreCredit)
            .WithMany()
            .HasForeignKey(x => x.StoreCreditId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.StoreCreditId);
    }
}
