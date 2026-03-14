using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetSettlements;

internal sealed class GetSettlementsQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSettlementsQuery, SettlementListResponse>
{
    public async Task<Result<SettlementListResponse>> Handle(
        GetSettlementsQuery request, CancellationToken cancellationToken)
    {
        var query = salesDb.Settlements.Where(s => !s.IsDeleted);

        if (request.SupplierId.HasValue)
            query = query.Where(s => s.SupplierId == request.SupplierId.Value);

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        var total = await query.CountAsync(cancellationToken);

        var settlementsRaw = await query
            .OrderByDescending(s => s.CreatedOn)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new
            {
                s.ExternalId, s.SupplierId, s.PeriodStart, s.PeriodEnd,
                s.TotalSalesAmount, s.CreditPercentageInStore, s.CashRedemptionPercentage,
                s.StoreCreditAmount, s.CashRedemptionAmount, s.StoreCommissionAmount,
                s.NetAmountToSupplier, s.Status,
                ItemCount = s.SaleItems.Count,
                s.PaidOn, s.PaidBy, s.CreatedOn, s.CreatedBy
            })
            .ToListAsync(cancellationToken);

        var supplierIds = settlementsRaw.Select(s => s.SupplierId).Distinct().ToList();
        var suppliers = await inventoryDb.Suppliers
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name, s.Initial })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var settlements = settlementsRaw.Select(s => new SettlementListItem(
            s.ExternalId, s.SupplierId,
            suppliers.GetValueOrDefault(s.SupplierId)?.Name ?? "",
            suppliers.GetValueOrDefault(s.SupplierId)?.Initial ?? "",
            s.PeriodStart, s.PeriodEnd, s.TotalSalesAmount,
            s.CreditPercentageInStore, s.CashRedemptionPercentage,
            s.StoreCreditAmount, s.CashRedemptionAmount, s.StoreCommissionAmount,
            s.NetAmountToSupplier, s.Status, s.ItemCount,
            s.PaidOn, s.PaidBy, s.CreatedOn, s.CreatedBy)).ToList();

        return new SettlementListResponse(total, request.Page, request.PageSize, settlements);
    }
}
