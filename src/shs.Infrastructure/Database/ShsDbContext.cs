using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database;

public class ShsDbContext : DbContext
{
    public ShsDbContext(DbContextOptions<ShsDbContext> options) : base(options) { }

    public DbSet<CashRegisterEntity> CashRegisters => Set<CashRegisterEntity>();
    public DbSet<SaleEntity> Sales => Set<SaleEntity>();
    public DbSet<SaleItemEntity> SaleItems => Set<SaleItemEntity>();
    public DbSet<SalePaymentEntity> SalePayments => Set<SalePaymentEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    // RBAC entities
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();

    // Inventory entities
    public DbSet<BrandEntity> Brands => Set<BrandEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<TagEntity> Tags => Set<TagEntity>();
    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();
    public DbSet<ReceptionEntity> Receptions => Set<ReceptionEntity>();
    public DbSet<ItemEntity> Items => Set<ItemEntity>();
    public DbSet<ItemPhotoEntity> ItemPhotos => Set<ItemPhotoEntity>();
    public DbSet<SupplierReturnEntity> SupplierReturns => Set<SupplierReturnEntity>();

    // Financial entities
    public DbSet<SettlementEntity> Settlements => Set<SettlementEntity>();
    public DbSet<StoreCreditEntity> StoreCredits => Set<StoreCreditEntity>();
    public DbSet<StoreCreditTransactionEntity> StoreCreditTransactions => Set<StoreCreditTransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShsDbContext).Assembly);
    }
}
