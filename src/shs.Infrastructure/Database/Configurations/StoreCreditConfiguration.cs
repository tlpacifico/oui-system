using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class StoreCreditConfiguration : IEntityTypeConfiguration<StoreCreditEntity>
{
    public void Configure(EntityTypeBuilder<StoreCreditEntity> b)
    {
        b.ToTable("StoreCredits");

        b.HasKey(x => x.Id);

        b.Property(x => x.ExternalId)
            .IsRequired();
        b.HasIndex(x => x.ExternalId)
            .IsUnique();

        b.Property(x => x.OriginalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.CurrentBalance)
            .HasPrecision(18, 2)
            .IsRequired();

        b.Property(x => x.Status)
            .IsRequired();

        b.Property(x => x.IssuedOn)
            .IsRequired();

        b.Property(x => x.IssuedBy)
            .HasMaxLength(200);

        b.Property(x => x.Notes)
            .HasMaxLength(2000);

        b.Property(x => x.CreatedBy)
            .HasMaxLength(200);

        b.Property(x => x.UpdatedBy)
            .HasMaxLength(200);

        b.Property(x => x.DeletedBy)
            .HasMaxLength(200);

        b.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        b.HasQueryFilter(x => !x.IsDeleted);

        // Relationships
        b.HasOne(x => x.Supplier)
            .WithMany(s => s.StoreCredits)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Transactions)
            .WithOne(t => t.StoreCredit)
            .HasForeignKey(t => t.StoreCreditId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        b.HasIndex(x => x.SupplierId);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.ExpiresOn);
    }
}
