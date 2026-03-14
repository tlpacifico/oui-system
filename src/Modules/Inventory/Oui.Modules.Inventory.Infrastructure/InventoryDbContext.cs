using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace Oui.Modules.Inventory.Infrastructure;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();
    public DbSet<BrandEntity> Brands => Set<BrandEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<ReceptionEntity> Receptions => Set<ReceptionEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<ItemPhotoEntity> ItemPhotos => Set<ItemPhotoEntity>();
    public DbSet<SupplierReturnEntity> SupplierReturns => Set<SupplierReturnEntity>();
    public DbSet<ReceptionApprovalTokenEntity> ReceptionApprovalTokens => Set<ReceptionApprovalTokenEntity>();
    public DbSet<ConsignmentItemEntity> ConsignmentItems => Set<ConsignmentItemEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Inventory);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
