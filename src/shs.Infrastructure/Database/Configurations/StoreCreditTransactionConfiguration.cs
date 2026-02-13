using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class StoreCreditTransactionConfiguration : IEntityTypeConfiguration<StoreCreditTransactionEntity>
{
    public void Configure(EntityTypeBuilder<StoreCreditTransactionEntity> b)
    {
        b.ToTable("StoreCreditTransactions");

        b.HasKey(x => x.Id);

        b.Property(x => x.ExternalId)
            .IsRequired();
        b.HasIndex(x => x.ExternalId)
            .IsUnique();

        b.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.BalanceAfter)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.TransactionType)
            .IsRequired();

        b.Property(x => x.TransactionDate)
            .IsRequired();

        b.Property(x => x.ProcessedBy)
            .HasMaxLength(200);

        b.Property(x => x.Notes)
            .HasMaxLength(2000);

        b.Property(x => x.CreatedBy)
            .HasMaxLength(200);

        // Relationships
        b.HasOne(x => x.StoreCredit)
            .WithMany(sc => sc.Transactions)
            .HasForeignKey(x => x.StoreCreditId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Sale)
            .WithMany()
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        b.HasIndex(x => x.StoreCreditId);
        b.HasIndex(x => x.TransactionDate);
        b.HasIndex(x => x.TransactionType);
    }
}
