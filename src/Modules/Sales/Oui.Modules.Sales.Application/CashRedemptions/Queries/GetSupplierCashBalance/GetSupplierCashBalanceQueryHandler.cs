using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashBalance;

internal sealed class GetSupplierCashBalanceQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSupplierCashBalanceQuery, SupplierCashBalanceResponse>
{
    public async Task<Result<SupplierCashBalanceResponse>> Handle(
        GetSupplierCashBalanceQuery request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<SupplierCashBalanceResponse>(CashRedemptionErrors.SupplierNotFound);

        var balance = await salesDb.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == request.SupplierId)
            .SumAsync(t => t.Amount, cancellationToken);

        return new SupplierCashBalanceResponse(
            request.SupplierId, supplier.Name, balance,
            supplier.CreditPercentageInStore, supplier.CashRedemptionPercentage);
    }
}
