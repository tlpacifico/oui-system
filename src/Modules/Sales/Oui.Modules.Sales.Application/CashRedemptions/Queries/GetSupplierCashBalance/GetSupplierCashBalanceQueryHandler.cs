using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
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

        // Saldo único: o valor resgatável em dinheiro deriva do crédito em loja ativo,
        // convertido à taxa PorcInDinheiro/PorcInLoja (ex.: 40/50 = 0.8).
        var creditBalance = await salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId && sc.Status == StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .SumAsync(sc => sc.CurrentBalance, cancellationToken);

        var rate = supplier.CreditPercentageInStore > 0
            ? supplier.CashRedemptionPercentage / supplier.CreditPercentageInStore
            : 0m;
        var maxCash = Math.Floor(creditBalance * rate * 100m) / 100m;

        return new SupplierCashBalanceResponse(
            request.SupplierId, supplier.Name, maxCash, creditBalance, rate,
            supplier.CreditPercentageInStore, supplier.CashRedemptionPercentage);
    }
}
