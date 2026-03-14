using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetPendingSettlementItems;

internal sealed class GetPendingSettlementItemsQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetPendingSettlementItemsQuery, List<PendingSettlementGroup>>
{
    public async Task<Result<List<PendingSettlementGroup>>> Handle(
        GetPendingSettlementItemsQuery request, CancellationToken cancellationToken)
    {
        var query = inventoryDb.Items
            .Include(i => i.Supplier)
            .Include(i => i.Brand)
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId != null);

        if (request.SupplierId.HasValue)
            query = query.Where(i => i.SupplierId == request.SupplierId.Value);

        if (request.StartDate.HasValue)
            query = query.Where(i => i.UpdatedOn >= ToUtc(request.StartDate.Value));

        if (request.EndDate.HasValue)
            query = query.Where(i => i.UpdatedOn < ToUtc(request.EndDate.Value).AddDays(1));

        var allItemIds = await query.Select(i => i.Id).ToListAsync(cancellationToken);
        var settledItemIds = await salesDb.SaleItems
            .Where(si => allItemIds.Contains(si.ItemId) && si.SettlementId != null)
            .Select(si => si.ItemId)
            .ToListAsync(cancellationToken);

        var items = await query
            .Where(i => !settledItemIds.Contains(i.Id))
            .Select(i => new PendingSettlementItem(
                i.Id, i.ExternalId, i.IdentificationNumber, i.Name,
                i.Brand!.Name, i.EvaluatedPrice, i.FinalSalePrice,
                i.CommissionPercentage, i.CommissionAmount,
                i.SupplierId, i.Supplier!.Name, i.Supplier.Initial, i.UpdatedOn))
            .ToListAsync(cancellationToken);

        var grouped = items
            .GroupBy(i => new { i.SupplierId, i.SupplierName, i.SupplierInitial })
            .Select(g => new PendingSettlementGroup(
                g.Key.SupplierId,
                g.Key.SupplierName,
                g.Key.SupplierInitial,
                g.Count(),
                g.Sum(i => i.FinalSalePrice ?? 0),
                g.OrderByDescending(i => i.UpdatedOn).ToList()))
            .OrderBy(g => g.SupplierName)
            .ToList();

        return grouped;
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
