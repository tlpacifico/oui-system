using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCreditBalance;

internal sealed class GetSupplierStoreCreditBalanceQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSupplierStoreCreditBalanceQuery, SupplierStoreCreditBalanceResponse>
{
    public async Task<Result<SupplierStoreCreditBalanceResponse>> Handle(
        GetSupplierStoreCreditBalanceQuery request, CancellationToken cancellationToken)
    {
        var exists = await inventoryDb.Suppliers.AnyAsync(s => s.Id == request.SupplierId && !s.IsDeleted, cancellationToken);
        if (!exists)
            return Result.Failure<SupplierStoreCreditBalanceResponse>(StoreCreditErrors.SupplierNotFound);

        var totalBalance = await salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId && sc.Status == StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .SumAsync(sc => sc.CurrentBalance, cancellationToken);

        return new SupplierStoreCreditBalanceResponse(request.SupplierId, totalBalance);
    }
}
