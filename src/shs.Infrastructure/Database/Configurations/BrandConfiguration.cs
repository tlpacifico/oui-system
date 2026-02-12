using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<BrandEntity>
{
    public void Configure(EntityTypeBuilder<BrandEntity> b)
    {
        b.ToTable("Brands");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Name);
        b.Property(x => x.Description).HasMaxLength(1000);
        b.Property(x => x.LogoUrl).HasMaxLength(500);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Items)
            .WithOne(x => x.Brand)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
