using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.Inventory.Infrastructure.Configurations;

internal class ColorConfiguration : IEntityTypeConfiguration<ColorEntity>
{
    public void Configure(EntityTypeBuilder<ColorEntity> b)
    {
        b.ToTable("Colors");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.HexCode).HasMaxLength(7);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Items)
            .WithMany(x => x.Colors)
            .UsingEntity(j => j.ToTable("ItemColors"));
    }
}
