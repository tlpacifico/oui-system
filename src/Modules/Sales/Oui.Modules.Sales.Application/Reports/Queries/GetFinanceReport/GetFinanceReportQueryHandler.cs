using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetFinanceReport;

internal sealed class GetFinanceReportQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetFinanceReportQuery, FinanceReportResponse>
{
    public async Task<Result<FinanceReportResponse>> Handle(
        GetFinanceReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var start = request.StartDate ?? now.Date.AddMonths(-1);
        var end = (request.EndDate ?? now.Date).AddDays(1);

        var salesInPeriod = await salesDb.Sales
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= start && s.SaleDate < end)
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

        var grossRevenue = salesInPeriod.Sum(s => s.TotalAmount);

        var allItemIds = salesInPeriod.SelectMany(s => s.Items).Select(si => si.ItemId).Distinct().ToList();
        var itemDetails = await inventoryDb.Items
            .Where(i => allItemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.AcquisitionType, i.CommissionAmount })
            .ToListAsync(cancellationToken);
        var itemDetailsById = itemDetails.ToDictionary(i => i.Id);

        var commissionRevenue = salesInPeriod
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.TryGetValue(si.ItemId, out var item) && item.AcquisitionType == AcquisitionType.Consignment)
            .Sum(si => itemDetailsById.TryGetValue(si.ItemId, out var item) ? item.CommissionAmount ?? 0 : 0);

        var settledItemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId != null)
            .Select(si => si.ItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingSettlements = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment && i.SupplierId != null)
            .Where(i => !settledItemIds.Contains(i.Id))
            .SumAsync(i => i.FinalSalePrice ?? 0, cancellationToken);

        var paidSettlements = await salesDb.Settlements
            .Where(s => !s.IsDeleted && s.Status == SettlementStatus.Paid && s.PaidOn >= start && s.PaidOn < end)
            .SumAsync(s => s.NetAmountToSupplier, cancellationToken);

        var projectedCashflow = grossRevenue - commissionRevenue - pendingSettlements;

        return new FinanceReportResponse(
            new ReportPeriod(start, end),
            grossRevenue, commissionRevenue, pendingSettlements, paidSettlements, projectedCashflow);
    }
}
