using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Infrastructure.Configurations;

internal class SupplierCashBalanceTransactionConfiguration : IEntityTypeConfiguration<SupplierCashBalanceTransactionEntity>
{
    public void Configure(EntityTypeBuilder<SupplierCashBalanceTransactionEntity> b)
    {
        b.ToTable("SupplierCashBalanceTransactions");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.TransactionType).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.ProcessedBy).HasMaxLength(256);
        b.Property(x => x.Notes).HasMaxLength(500);

        // Cross-module FK to Inventory (Supplier) — no navigation, just FK property + index
        b.Property(x => x.SupplierId).IsRequired();
        b.HasIndex(x => x.SupplierId);

        // Intra-module relationship
        b.HasOne(x => x.Settlement)
            .WithMany(x => x.CashBalanceTransactions)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(x => x.SettlementId);
        b.HasIndex(x => x.TransactionDate);
    }
}
