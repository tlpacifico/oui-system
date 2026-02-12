using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class ConsignmentItemConfiguration : IEntityTypeConfiguration<ConsignmentItemEntity>
{
    public void Configure(EntityTypeBuilder<ConsignmentItemEntity> b)
    {
        b.ToTable("ConsignmentItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.IdentificationNumber).HasMaxLength(32).IsRequired();
        b.Property(x => x.Name).HasMaxLength(500).IsRequired();
        b.Property(x => x.EvaluatedValue).HasPrecision(18, 2);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
