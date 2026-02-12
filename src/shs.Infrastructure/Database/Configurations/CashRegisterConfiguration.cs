using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class CashRegisterConfiguration : IEntityTypeConfiguration<CashRegisterEntity>
{
    public void Configure(EntityTypeBuilder<CashRegisterEntity> b)
    {
        b.ToTable("CashRegisters");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();
        b.Property(x => x.OperatorUserId).HasMaxLength(256).IsRequired();
        b.Property(x => x.OperatorName).HasMaxLength(256).IsRequired();
        b.Property(x => x.OpeningAmount).HasPrecision(18, 2);
        b.Property(x => x.ClosingAmount).HasPrecision(18, 2);
        b.Property(x => x.ExpectedAmount).HasPrecision(18, 2);
        b.Property(x => x.Discrepancy).HasPrecision(18, 2);
        b.Property(x => x.DiscrepancyNotes).HasMaxLength(1000);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.IsDeleted).HasDefaultValue(false);
        b.Property(x => x.DeletedBy).HasMaxLength(256);
        b.HasMany(x => x.Sales).WithOne(x => x.CashRegister).HasForeignKey(x => x.CashRegisterId).OnDelete(DeleteBehavior.Restrict);
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}
