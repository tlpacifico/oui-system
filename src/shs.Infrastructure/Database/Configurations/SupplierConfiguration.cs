using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<SupplierEntity>
{
    public void Configure(EntityTypeBuilder<SupplierEntity> b)
    {
        b.ToTable("Suppliers");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Name).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.Name);
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.HasIndex(x => x.Email);
        b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.TaxNumber).HasMaxLength(20);
        b.HasIndex(x => x.TaxNumber);
        b.Property(x => x.Initial).HasMaxLength(5).IsRequired();
        b.HasIndex(x => x.Initial).IsUnique();
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Receptions)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Items)
            .WithOne(x => x.Supplier)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
