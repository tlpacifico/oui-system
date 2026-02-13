using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SupplierReturnConfiguration : IEntityTypeConfiguration<SupplierReturnEntity>
{
    public void Configure(EntityTypeBuilder<SupplierReturnEntity> b)
    {
        b.ToTable("SupplierReturns");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Notes).HasMaxLength(2000);

        b.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
            .WithOne(i => i.SupplierReturn)
            .HasForeignKey(i => i.SupplierReturnId)
            .OnDelete(DeleteBehavior.SetNull);

        // Soft delete filter
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
