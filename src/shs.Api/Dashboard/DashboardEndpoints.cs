using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Dashboard;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard");

        group.MapGet("/", GetDashboard)
            .RequirePermission("dashboard.view");
    }

    private static async Task<IResult> GetDashboard(
        ShsDbContext db,
        [FromQuery] string period = "today",
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var tomorrow = today.AddDays(1);

        // ── Sales Today ──
        var todaySales = await db.Sales
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .Include(s => s.Payments)
            .Include(s => s.Items)
            .ToListAsync(ct);

        var salesToday = new
        {
            count = todaySales.Count,
            revenue = todaySales.Sum(s => s.TotalAmount),
            averageTicket = todaySales.Count > 0 ? todaySales.Sum(s => s.TotalAmount) / todaySales.Count : 0m
        };

        // ── Sales Month ──
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart;

        var monthSales = await db.Sales
            .Where(s => s.SaleDate >= monthStart && s.SaleDate < monthEnd && s.Status == SaleStatus.Active)
            .ToListAsync(ct);

        var lastMonthSales = await db.Sales
            .Where(s => s.SaleDate >= lastMonthStart && s.SaleDate < lastMonthEnd && s.Status == SaleStatus.Active)
            .ToListAsync(ct);

        var monthRevenue = monthSales.Sum(s => s.TotalAmount);
        var lastMonthRevenue = lastMonthSales.Sum(s => s.TotalAmount);
        var growthPercent = lastMonthRevenue > 0
            ? (decimal)((double)(monthRevenue - lastMonthRevenue) / (double)lastMonthRevenue * 100)
            : (monthRevenue > 0 ? 100m : 0m);

        var salesMonth = new
        {
            count = monthSales.Count,
            revenue = monthRevenue,
            averageTicket = monthSales.Count > 0 ? monthRevenue / monthSales.Count : 0m,
            growthPercent
        };

        // ── Inventory ──
        var toSellItems = await db.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.ToSell)
            .Select(i => new { i.EvaluatedPrice, i.DaysInStock, i.CreatedOn })
            .ToListAsync(ct);

        var totalItems = toSellItems.Count;
        var totalValue = toSellItems.Sum(i => i.EvaluatedPrice);
        var stagnantCount = toSellItems.Count(i =>
        {
            var days = i.DaysInStock > 0 ? i.DaysInStock : (int)(now - i.CreatedOn).TotalDays;
            return days >= 30;
        });

        var inventory = new
        {
            totalItems,
            totalValue,
            stagnantCount
        };

        // ── Pending Settlements ──
        var pendingGroups = await db.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId != null)
            .Where(i => !db.SaleItems.Any(si => si.ItemId == i.Id && si.SettlementId != null))
            .GroupBy(i => i.SupplierId)
            .Select(g => new
            {
                SupplierId = g.Key,
                Total = g.Sum(i => i.FinalSalePrice ?? 0)
            })
            .ToListAsync(ct);

        var pendingSettlements = new
        {
            totalAmount = pendingGroups.Sum(g => g.Total),
            suppliersCount = pendingGroups.Count
        };

        // ── Top Selling Items (this week) ──
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        if (today.DayOfWeek == DayOfWeek.Sunday) weekStart = weekStart.AddDays(-7);
        var weekEnd = weekStart.AddDays(7);

        var topSellingItems = await db.SaleItems
            .Where(si => si.Sale != null &&
                        si.Sale.SaleDate >= weekStart &&
                        si.Sale.SaleDate < weekEnd &&
                        si.Sale.Status == SaleStatus.Active)
            .Include(si => si.Item)
                .ThenInclude(i => i!.Brand)
            .OrderByDescending(si => si.Sale!.SaleDate)
            .Take(5)
            .Select(si => new
            {
                si.Item!.Name,
                Brand = si.Item.Brand!.Name,
                si.FinalPrice,
                SoldDate = si.Sale!.SaleDate
            })
            .ToListAsync(ct);

        // ── Alerts ──
        var stagnant30 = toSellItems.Count(i =>
        {
            var days = i.DaysInStock > 0 ? i.DaysInStock : (int)(now - i.CreatedOn).TotalDays;
            return days >= 30 && days < 45;
        });
        var stagnant45 = toSellItems.Count(i =>
        {
            var days = i.DaysInStock > 0 ? i.DaysInStock : (int)(now - i.CreatedOn).TotalDays;
            return days >= 45 && days < 60;
        });
        var stagnant60 = toSellItems.Count(i =>
        {
            var days = i.DaysInStock > 0 ? i.DaysInStock : (int)(now - i.CreatedOn).TotalDays;
            return days >= 60;
        });

        var consignmentPeriodDays = 60;
        var expiryStart = today.AddDays(-consignmentPeriodDays);
        var expiryEnd = today.AddDays(-consignmentPeriodDays + 7);
        var expiringConsignments = await db.Items
            .Where(i => !i.IsDeleted &&
                       (i.Status == ItemStatus.ToSell || i.Status == ItemStatus.Evaluated || i.Status == ItemStatus.AwaitingAcceptance) &&
                       i.ReceptionId != null)
            .Join(db.Receptions,
                i => i.ReceptionId,
                r => r.Id,
                (i, r) => new { Item = i, Reception = r })
            .Where(x => !x.Reception.IsDeleted &&
                        x.Reception.ReceptionDate.Date >= expiryStart &&
                        x.Reception.ReceptionDate.Date < expiryEnd)
            .CountAsync(ct);

        var openRegisters = await db.CashRegisters
            .Where(r => r.Status == CashRegisterStatus.Open)
            .Include(r => r.Sales.Where(s => !s.IsDeleted && s.Status == SaleStatus.Active))
            .Select(r => new
            {
                r.OperatorName,
                r.OpenedAt,
                SalesCount = r.Sales.Count
            })
            .ToListAsync(ct);

        var alerts = new
        {
            expiringConsignments,
            stagnantItems30 = stagnant30,
            stagnantItems45 = stagnant45,
            stagnantItems60 = stagnant60,
            openRegisters
        };

        // ── Sales Chart (last 7 or 30 days) ──
        var chartDays = period == "month" ? 30 : 7;
        var chartStart = today.AddDays(-chartDays);

        var salesByDate = await db.Sales
            .Where(s => s.SaleDate >= chartStart && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .GroupBy(s => s.SaleDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Revenue = g.Sum(s => s.TotalAmount),
                Count = g.Count()
            })
            .ToListAsync(ct);

        var allDates = Enumerable.Range(0, chartDays)
            .Select(i => chartStart.AddDays(i).Date)
            .ToList();

        var salesChart = allDates.Select(d =>
        {
            var match = salesByDate.FirstOrDefault(s => s.Date == d);
            return new
            {
                date = d.ToString("yyyy-MM-dd"),
                revenue = match?.Revenue ?? 0,
                count = match?.Count ?? 0
            };
        }).ToList();

        return Results.Ok(new
        {
            salesToday,
            salesMonth,
            inventory,
            pendingSettlements,
            topSellingItems,
            alerts,
            salesChart
        });
    }
}
