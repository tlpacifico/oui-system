using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace Oui.Modules.System.Infrastructure.Configurations;

internal class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntity>
{
    public void Configure(EntityTypeBuilder<AuditLogEntity> b)
    {
        b.ToTable("AuditLogs");
        b.HasKey(x => x.Id);

        b.Property(x => x.EntityName).HasMaxLength(200).IsRequired();
        b.HasIndex(x => x.EntityName);

        b.Property(x => x.EntityId).HasMaxLength(100).IsRequired();

        b.Property(x => x.Action).HasMaxLength(20).IsRequired();
        b.HasIndex(x => x.Action);

        b.Property(x => x.OldValues).HasColumnType("jsonb");
        b.Property(x => x.NewValues).HasColumnType("jsonb");
        b.Property(x => x.ChangedColumns).HasColumnType("jsonb");

        b.Property(x => x.UserEmail).HasMaxLength(256);
        b.HasIndex(x => x.UserEmail);

        b.Property(x => x.Timestamp).IsRequired();
        b.HasIndex(x => x.Timestamp);
    }
}
