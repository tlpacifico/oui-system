using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace Oui.Modules.Ecommerce.Infrastructure;

public sealed class EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) : DbContext(options)
{
    public DbSet<EcommerceProductEntity> EcommerceProducts => Set<EcommerceProductEntity>();
    public DbSet<EcommerceProductPhotoEntity> EcommerceProductPhotos => Set<EcommerceProductPhotoEntity>();
    public DbSet<EcommerceOrderEntity> EcommerceOrders => Set<EcommerceOrderEntity>();
    public DbSet<EcommerceOrderItemEntity> EcommerceOrderItems => Set<EcommerceOrderItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Ecommerce);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EcommerceDbContext).Assembly);
    }
}
