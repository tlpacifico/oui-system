using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class ReceptionConfiguration : IEntityTypeConfiguration<ReceptionEntity>
{
    public void Configure(EntityTypeBuilder<ReceptionEntity> b)
    {
        b.ToTable("Receptions");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.ReceptionDate).IsRequired();
        b.HasIndex(x => x.ReceptionDate);
        b.Property(x => x.ItemCount).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.Status);
        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.EvaluatedBy).HasMaxLength(256);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);

        b.HasMany(x => x.Items)
            .WithOne(x => x.Reception)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
