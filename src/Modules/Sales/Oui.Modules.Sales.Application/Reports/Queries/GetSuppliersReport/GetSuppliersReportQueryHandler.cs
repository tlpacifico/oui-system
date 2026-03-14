using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetSuppliersReport;

internal sealed class GetSuppliersReportQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSuppliersReportQuery, SuppliersReportResponse>
{
    public async Task<Result<SuppliersReportResponse>> Handle(
        GetSuppliersReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var start = request.StartDate ?? now.Date.AddMonths(-1);
        var end = (request.EndDate ?? now.Date).AddDays(1);

        var suppliers = await inventoryDb.Suppliers
            .Where(s => !s.IsDeleted)
            .Select(s => new { s.Id, s.ExternalId, s.Name, s.Initial })
            .ToListAsync(cancellationToken);

        var soldItemsInPeriod = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold && i.SupplierId != null &&
                        i.SoldAt >= start && i.SoldAt < end)
            .Select(i => new { i.SupplierId, i.FinalSalePrice, i.SoldAt, i.CreatedOn, i.Id })
            .ToListAsync(cancellationToken);

        var returnedItems = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Returned && i.SupplierId != null &&
                        i.ReturnedAt >= start && i.ReturnedAt < end)
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var settledItemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId != null)
            .Select(si => si.ItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingBySupplier = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold && i.AcquisitionType == AcquisitionType.Consignment && i.SupplierId != null)
            .Where(i => !settledItemIds.Contains(i.Id))
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, PendingAmount = g.Sum(i => i.FinalSalePrice ?? 0) })
            .ToListAsync(cancellationToken);

        var ranking = suppliers.Select(s => new
        {
            s.Id, s.ExternalId, s.Name, s.Initial,
            Revenue = soldItemsInPeriod.Where(i => i.SupplierId == s.Id).Sum(i => i.FinalSalePrice ?? 0),
            SoldCount = soldItemsInPeriod.Count(i => i.SupplierId == s.Id),
            ReturnedCount = returnedItems.FirstOrDefault(r => r.SupplierId == s.Id)?.Count ?? 0,
            PendingAmount = pendingBySupplier.FirstOrDefault(p => p.SupplierId == s.Id)?.PendingAmount ?? 0
        })
        .Where(x => x.SoldCount > 0 || x.PendingAmount > 0)
        .OrderByDescending(x => x.Revenue)
        .Select(x => new SupplierRankingItem(
            x.Id, x.ExternalId, x.Name, x.Initial, x.Revenue, x.SoldCount, x.ReturnedCount,
            x.SoldCount > 0 ? (double)x.ReturnedCount / x.SoldCount * 100 : 0,
            x.PendingAmount,
            x.SoldCount > 0
                ? soldItemsInPeriod.Where(i => i.SupplierId == x.Id && i.SoldAt.HasValue)
                    .Select(i => (i.SoldAt!.Value - i.CreatedOn).TotalDays).DefaultIfEmpty(0).Average()
                : 0.0))
        .ToList();

        return new SuppliersReportResponse(new ReportPeriod(start, end), ranking);
    }
}
