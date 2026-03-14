using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Infrastructure;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<CashRegisterEntity> CashRegisters => Set<CashRegisterEntity>();
    public DbSet<SaleEntity> Sales => Set<SaleEntity>();
    public DbSet<SaleItemEntity> SaleItems => Set<SaleItemEntity>();
    public DbSet<SalePaymentEntity> SalePayments => Set<SalePaymentEntity>();
    public DbSet<SettlementEntity> Settlements => Set<SettlementEntity>();
    public DbSet<StoreCreditEntity> StoreCredits => Set<StoreCreditEntity>();
    public DbSet<StoreCreditTransactionEntity> StoreCreditTransactions => Set<StoreCreditTransactionEntity>();
    public DbSet<SupplierCashBalanceTransactionEntity> SupplierCashBalanceTransactions => Set<SupplierCashBalanceTransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Sales);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
    }
}
