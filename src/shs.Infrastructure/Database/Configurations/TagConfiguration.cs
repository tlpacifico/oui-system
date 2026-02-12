using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> b)
    {
        b.ToTable("Tags");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.Property(x => x.Color).HasMaxLength(7); // #RRGGBB
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        // Many-to-Many with Items (EF Core auto-creates join table)
        b.HasMany(x => x.Items)
            .WithMany(x => x.Tags)
            .UsingEntity(j => j.ToTable("ItemTags"));
    }
}
