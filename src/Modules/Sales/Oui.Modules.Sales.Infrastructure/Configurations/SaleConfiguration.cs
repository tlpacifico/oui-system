using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Infrastructure.Configurations;

internal class SaleConfiguration : IEntityTypeConfiguration<SaleEntity>
{
    public void Configure(EntityTypeBuilder<SaleEntity> b)
    {
        b.ToTable("Sales");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.SaleNumber).HasMaxLength(32).IsRequired();
        b.HasIndex(x => x.SaleNumber).IsUnique();
        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
        b.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.DiscountReason).HasMaxLength(500);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.Status);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Items)
            .WithOne(x => x.Sale)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(x => x.Payments)
            .WithOne(x => x.Sale)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
