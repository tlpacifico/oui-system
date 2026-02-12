using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace shs.Infrastructure.Database;

public class ShsDbContext : DbContext
{
    public ShsDbContext(DbContextOptions<ShsDbContext> options) : base(options) { }

    public DbSet<ConsignmentItemEntity> ConsignmentItems => Set<ConsignmentItemEntity>();
    public DbSet<CashRegisterEntity> CashRegisters => Set<CashRegisterEntity>();
    public DbSet<SaleEntity> Sales => Set<SaleEntity>();
    public DbSet<SaleItemEntity> SaleItems => Set<SaleItemEntity>();
    public DbSet<SalePaymentEntity> SalePayments => Set<SalePaymentEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShsDbContext).Assembly);
    }
}
