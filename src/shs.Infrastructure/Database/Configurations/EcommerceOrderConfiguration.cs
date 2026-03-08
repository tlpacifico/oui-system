using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class EcommerceOrderConfiguration : IEntityTypeConfiguration<EcommerceOrderEntity>
{
    public void Configure(EntityTypeBuilder<EcommerceOrderEntity> b)
    {
        b.ToTable("EcommerceOrders");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();

        b.Property(x => x.OrderNumber).HasMaxLength(32).IsRequired();
        b.HasIndex(x => x.OrderNumber).IsUnique();

        b.Property(x => x.CustomerName).HasMaxLength(300).IsRequired();
        b.Property(x => x.CustomerEmail).HasMaxLength(300).IsRequired();
        b.Property(x => x.CustomerPhone).HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.CancellationReason).HasMaxLength(1000);

        // Indexes
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.CustomerEmail);
        b.HasIndex(x => x.ExpiresAt);

        // Soft delete
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
