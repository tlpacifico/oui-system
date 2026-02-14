using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SupplierCashBalanceTransactionConfiguration : IEntityTypeConfiguration<SupplierCashBalanceTransactionEntity>
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

        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.SettlementId);
        b.HasIndex(x => x.TransactionDate);

        b.HasOne(x => x.Supplier)
            .WithMany(x => x.CashBalanceTransactions)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Settlement)
            .WithMany(x => x.CashBalanceTransactions)
            .HasForeignKey(x => x.SettlementId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
