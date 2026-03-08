using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database.Configurations;

public class EcommerceOrderItemConfiguration : IEntityTypeConfiguration<EcommerceOrderItemEntity>
{
    public void Configure(EntityTypeBuilder<EcommerceOrderItemEntity> b)
    {
        b.ToTable("EcommerceOrderItems");
        b.HasKey(x => x.Id);
        b.Property(x => x.ExternalId).IsRequired();
        b.HasIndex(x => x.ExternalId).IsUnique();

        b.Property(x => x.ProductTitle).HasMaxLength(500).IsRequired();
        b.Property(x => x.Price).HasPrecision(18, 2);

        b.HasIndex(x => x.ProductId);
        b.HasIndex(x => x.ItemId);

        b.HasOne(x => x.Product)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
