using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<ItemEntity>
{
    public void Configure(EntityTypeBuilder<ItemEntity> b)
    {
        b.ToTable("Items");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();

        // Identification
        b.Property(x => x.IdentificationNumber).HasMaxLength(32).IsRequired();
        b.HasIndex(x => x.IdentificationNumber).IsUnique();
        b.Property(x => x.Name).HasMaxLength(500).IsRequired();
        b.Property(x => x.Description).HasMaxLength(2000);

        // Classification
        b.Property(x => x.Size).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Size);
        b.Property(x => x.Color).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Color);
        b.Property(x => x.Composition).HasMaxLength(500);
        b.Property(x => x.Condition).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.Condition);

        // Pricing
        b.Property(x => x.EvaluatedPrice).HasPrecision(18, 2);
        b.Property(x => x.CostPrice).HasPrecision(18, 2);
        b.Property(x => x.FinalSalePrice).HasPrecision(18, 2);
        b.Property(x => x.CommissionPercentage).HasPrecision(5, 2);
        b.Property(x => x.CommissionAmount).HasPrecision(18, 2);

        // Status
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.Status);
        b.Property(x => x.AcquisitionType).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.AcquisitionType);
        b.Property(x => x.Origin).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.Origin);

        // Rejection
        b.Property(x => x.RejectionReason).HasMaxLength(1000);

        // Indexes for filtering/searching
        b.HasIndex(x => x.BrandId);
        b.HasIndex(x => x.CategoryId);
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.ReceptionId);
        b.HasIndex(x => x.SaleId);
        b.HasIndex(x => x.DaysInStock);
        b.HasIndex(x => x.EvaluatedPrice);

        // Soft delete
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        // Navigation - defined in other configurations
        b.HasMany(x => x.Photos)
            .WithOne(x => x.Item)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
