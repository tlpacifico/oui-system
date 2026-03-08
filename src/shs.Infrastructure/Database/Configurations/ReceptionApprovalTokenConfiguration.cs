using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class ReceptionApprovalTokenConfiguration : IEntityTypeConfiguration<ReceptionApprovalTokenEntity>
{
    public void Configure(EntityTypeBuilder<ReceptionApprovalTokenEntity> b)
    {
        b.ToTable("ReceptionApprovalTokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.Token).IsRequired().HasMaxLength(64);
        b.HasIndex(x => x.Token).IsUnique();
        b.Property(x => x.ExpiresAt).IsRequired();
        b.Property(x => x.ApprovedBy).HasMaxLength(256);
        b.Property(x => x.IsUsed).HasDefaultValue(false);

        b.HasOne(x => x.Reception)
            .WithMany(x => x.ApprovalTokens)
            .HasForeignKey(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
