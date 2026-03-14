using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.System.Infrastructure.Configurations;

internal class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSettingEntity>
{
    public void Configure(EntityTypeBuilder<SystemSettingEntity> b)
    {
        b.ToTable("SystemSettings");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Key).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.Key).IsUnique();
        b.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        b.Property(x => x.ValueType).HasMaxLength(50);
        b.Property(x => x.Module).HasMaxLength(100);
        b.HasIndex(x => x.Module);
        b.Property(x => x.DisplayName).HasMaxLength(300);
        b.Property(x => x.Description).HasMaxLength(1000);
    }
}
