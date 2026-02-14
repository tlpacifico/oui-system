using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SalePaymentConfiguration : IEntityTypeConfiguration<SalePaymentEntity>
{
    public void Configure(EntityTypeBuilder<SalePaymentEntity> b)
    {
        b.ToTable("SalePayments");
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasPrecision(18, 2);
        b.Property(x => x.Reference).HasMaxLength(256);
        b.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(32);

        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.StoreCreditId);

        b.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.StoreCredit)
            .WithMany()
            .HasForeignKey(x => x.StoreCreditId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
