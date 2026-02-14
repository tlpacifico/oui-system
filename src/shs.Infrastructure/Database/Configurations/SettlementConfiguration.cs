using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SettlementConfiguration : IEntityTypeConfiguration<SettlementEntity>
{
    public void Configure(EntityTypeBuilder<SettlementEntity> b)
    {
        b.ToTable("Settlements");

        b.HasKey(x => x.Id);

        b.Property(x => x.ExternalId)
            .IsRequired();
        b.HasIndex(x => x.ExternalId)
            .IsUnique();

        b.Property(x => x.TotalSalesAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.CommissionPercentage)
            .HasPrecision(5, 2);

        b.Property(x => x.CreditPercentageInStore)
            .HasPrecision(5, 2)
            .IsRequired();

        b.Property(x => x.CashRedemptionPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        b.Property(x => x.StoreCreditAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.CashRedemptionAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.StoreCommissionAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.NetAmountToSupplier)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.PaymentMethod)
            .IsRequired();

        b.Property(x => x.Status)
            .IsRequired();

        b.Property(x => x.PeriodStart)
            .IsRequired();

        b.Property(x => x.PeriodEnd)
            .IsRequired();

        b.Property(x => x.Notes)
            .HasMaxLength(2000);

        b.Property(x => x.CreatedBy)
            .HasMaxLength(200);

        b.Property(x => x.UpdatedBy)
            .HasMaxLength(200);

        b.Property(x => x.PaidBy)
            .HasMaxLength(200);

        b.Property(x => x.DeletedBy)
            .HasMaxLength(200);

        b.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        b.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        b.HasOne(x => x.Supplier)
            .WithMany(s => s.Settlements)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.StoreCredit)
            .WithOne(sc => sc.SourceSettlement)
            .HasForeignKey<SettlementEntity>(x => x.StoreCreditId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.SaleItems)
            .WithOne(si => si.Settlement)
            .HasForeignKey(si => si.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => new { x.PeriodStart, x.PeriodEnd });
    }
}
