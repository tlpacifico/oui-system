using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Dashboard.Queries.GetDashboard;

internal sealed class GetDashboardQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<Result<DashboardResponse>> Handle(
        GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        // ── Sales Today ──
        var todaySales = await salesDb.Sales
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .Include(s => s.Payments)
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

        var salesToday = new DashboardSalesToday(
            todaySales.Count,
            todaySales.Sum(s => s.TotalAmount),
            todaySales.Count > 0 ? todaySales.Sum(s => s.TotalAmount) / todaySales.Count : 0m);

        // ── Sales Month ──
        var monthStart = DateTime.SpecifyKind(new DateTime(today.Year, today.Month, 1), DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);

        var monthSales = await salesDb.Sales
            .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd && s.Status == SaleStatus.Active)
            .ToListAsync(cancellationToken);

        var lastMonthSales = await salesDb.Sales
            .Where(s => s.SaleDate >= lastMonthStart && s.SaleDate < monthStart && s.Status == SaleStatus.Active)
            .ToListAsync(cancellationToken);

        var monthRevenue = monthSales.Sum(s => s.TotalAmount);
        var lastMonthRevenue = lastMonthSales.Sum(s => s.TotalAmount);
        var growthPercent = lastMonthRevenue > 0
            ? (decimal)((double)(monthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100)
            : (monthRevenue > 0 ? 100m : 0m);

        var salesMonth = new DashboardSalesMonth(
            monthSales.Count,
            monthRevenue,
            monthSales.Count > 0 ? monthRevenue / monthSales.Count : 0m,
            growthPercent);

        // ── Inventory ──
        var toSellItems = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.ToSell)
            .Select(i => new { i.EvaluatedPrice, i.DaysInStock, i.CreatedOn })
            .ToListAsync(cancellationToken);

        var stagnantCount = toSellItems.Count(i =>
        {
            var days = i.DaysInStock > 0 ? i.DaysInStock : (int)(now - i.CreatedOn).TotalDays;
            return days >= 30;
        });

        var inventory = new DashboardInventory(
            toSellItems.Count,
            toSellItems.Sum(i => i.EvaluatedPrice),
            stagnantCount);

        // ── Pending Settlements ──
        var settledItemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId != null)
            .Select(si => si.ItemId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pendingGroups = await inventoryDb.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId != null)
            .Where(i => !settledItemIds.Contains(i.Id))
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, Total = g.Sum(i => i.FinalSalePrice ?? 0) })
            .ToListAsync(cancellationToken);

        var pendingSettlements = new DashboardPendingSettlements(
            pendingGroups.Sum(g => g.Total),
            pendingGroups.Count);

        // ── Top Selling Items (this week) ──
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        if (today.DayOfWeek == DayOfWeek.Sunday) weekStart = weekStart.AddDays(-7);
        var weekEnd = weekStart.AddDays(7);

        var topSaleItems = await salesDb.SaleItems
            .Where(si => si.Sale != null &&
                        si.Sale.SaleDate >= weekStart &&
                        si.Sale.SaleDate < weekEnd &&
                        si.Sale.Status == SaleStatus.Active)
            .OrderByDescending(si => si.Sale!.SaleDate)
            .Take(5)
            .Select(si => new { si.ItemId, si.FinalPrice, SoldDate = si.Sale!.SaleDate })
            .ToListAsync(cancellationToken);

        var topItemIds = topSaleItems.Select(si => si.ItemId).ToList();
        var topItemDetails = await inventoryDb.Items
            .Where(i => topItemIds.Contains(i.Id))
            .Include(i => i.Brand)
            .Select(i => new { i.Id, i.Name, BrandName = i.Brand!.Name })
            .ToListAsync(cancellationToken);

        var topSellingItems = topSaleItems.Select(si =>
        {
            var item = topItemDetails.FirstOrDefault(i => i.Id == si.ItemId);
            return new DashboardTopSellingItem(
                item?.Name ?? "Unknown",
                item?.BrandName ?? "Unknown",
                si.FinalPrice,
                si.SoldDate);
        }).ToList();

        // ── Alerts ──
        int GetDays(int daysInStock, DateTime createdOn) =>
            daysInStock > 0 ? daysInStock : (int)(now - createdOn).TotalDays;

        var stagnant30 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 30 && d < 45; });
        var stagnant45 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 45 && d < 60; });
        var stagnant60 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 60; });

        var consignmentPeriodDays = 60;
        var expiryStart = today.AddDays(-consignmentPeriodDays);
        var expiryEnd = today.AddDays(-consignmentPeriodDays + 7);
        var expiringConsignments = await inventoryDb.Items
            .Where(i => !i.IsDeleted &&
                       (i.Status == ItemStatus.ToSell || i.Status == ItemStatus.Evaluated || i.Status == ItemStatus.AwaitingAcceptance) &&
                       i.ReceptionId != null)
            .Join(inventoryDb.Receptions,
                i => i.ReceptionId,
                r => r.Id,
                (i, r) => new { Item = i, Reception = r })
            .Where(x => !x.Reception.IsDeleted &&
                        x.Reception.ReceptionDate.Date >= expiryStart &&
                        x.Reception.ReceptionDate.Date < expiryEnd)
            .CountAsync(cancellationToken);

        var openRegisters = await salesDb.CashRegisters
            .Where(r => r.Status == CashRegisterStatus.Open)
            .Include(r => r.Sales.Where(s => !s.IsDeleted && s.Status == SaleStatus.Active))
            .Select(r => new DashboardOpenRegister(r.OperatorName, r.OpenedAt, r.Sales.Count))
            .ToListAsync(cancellationToken);

        var alerts = new DashboardAlerts(
            expiringConsignments,
            stagnant30,
            stagnant45,
            stagnant60,
            openRegisters);

        // ── Sales Chart ──
        var chartDays = request.Period == "month" ? 30 : 7;
        var chartStart = today.AddDays(-chartDays);

        var salesByDate = await salesDb.Sales
            .Where(s => s.SaleDate >= chartStart && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new { Date = g.Key, Revenue = g.Sum(s => s.TotalAmount), Count = g.Count() })
            .ToListAsync(cancellationToken);

        var allDates = Enumerable.Range(0, chartDays)
            .Select(i => chartStart.AddDays(i).Date)
            .ToList();

        var salesChart = allDates.Select(d =>
        {
            var match = salesByDate.FirstOrDefault(s => s.Date == d);
            return new DashboardChartPoint(d.ToString("yyyy-MM-dd"), match?.Revenue ?? 0, match?.Count ?? 0);
        }).ToList();

        return new DashboardResponse(
            salesToday,
            salesMonth,
            inventory,
            pendingSettlements,
            topSellingItems,
            alerts,
            salesChart);
    }
}
