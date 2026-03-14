using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Inventory.Infrastructure.Configurations;

internal class SupplierReturnConfiguration : IEntityTypeConfiguration<SupplierReturnEntity>
{
    public void Configure(EntityTypeBuilder<SupplierReturnEntity> b)
    {
        b.ToTable("SupplierReturns");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasOne(x => x.Supplier)
            .WithMany()
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
            .WithOne(x => x.SupplierReturn)
            .HasForeignKey(x => x.SupplierReturnId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
