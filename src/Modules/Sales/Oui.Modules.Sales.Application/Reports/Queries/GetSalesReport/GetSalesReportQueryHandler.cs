using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetSalesReport;

internal sealed class GetSalesReportQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSalesReportQuery, SalesReportResponse>
{
    public async Task<Result<SalesReportResponse>> Handle(
        GetSalesReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var start = request.StartDate ?? now.Date.AddMonths(-1);
        var end = (request.EndDate ?? now.Date).AddDays(1);

        var sales = await salesDb.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= start && s.SaleDate < end)
            .ToListAsync(cancellationToken);

        var allItemIds = sales.SelectMany(s => s.Items).Select(si => si.ItemId).Distinct().ToList();

        var itemDetails = await inventoryDb.Items
            .Where(i => allItemIds.Contains(i.Id))
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Select(i => new
            {
                i.Id, i.BrandId, BrandName = i.Brand!.Name,
                i.CategoryId, CategoryName = i.Category != null ? i.Category.Name : (string?)null,
                i.AcquisitionType, i.CommissionAmount
            })
            .ToListAsync(cancellationToken);

        var itemDetailsById = itemDetails.ToDictionary(i => i.Id);

        var filteredSales = sales.AsEnumerable();
        if (request.BrandId.HasValue)
            filteredSales = filteredSales.Where(s => s.Items.Any(si =>
                itemDetailsById.TryGetValue(si.ItemId, out var item) && item.BrandId == request.BrandId.Value));
        if (request.CategoryId.HasValue)
            filteredSales = filteredSales.Where(s => s.Items.Any(si =>
                itemDetailsById.TryGetValue(si.ItemId, out var item) && item.CategoryId == request.CategoryId.Value));

        var salesList = filteredSales.ToList();
        var revenue = salesList.Sum(s => s.TotalAmount);
        var salesCount = salesList.Count;
        var avgTicket = salesCount > 0 ? revenue / salesCount : 0m;

        var paymentBreakdown = salesList
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key.ToString(), g => new PaymentBreakdownItem(g.Count(), g.Sum(p => p.Amount)));

        var dailySalesChart = salesList
            .GroupBy(s => s.SaleDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailySalesChartItem(g.Key.ToString("yyyy-MM-dd"), g.Sum(s => s.TotalAmount), g.Count()))
            .ToList();

        var topBrands = salesList
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.ContainsKey(si.ItemId))
            .GroupBy(si => new { itemDetailsById[si.ItemId].BrandId, itemDetailsById[si.ItemId].BrandName })
            .Select(g => new TopBrandResult(g.Key.BrandName, g.Sum(si => si.FinalPrice), g.Count()))
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        var topCategories = salesList
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.TryGetValue(si.ItemId, out var item) && item.CategoryId != null)
            .GroupBy(si => new { itemDetailsById[si.ItemId].CategoryId, itemDetailsById[si.ItemId].CategoryName })
            .Select(g => new TopCategoryResult(g.Key.CategoryName, g.Sum(si => si.FinalPrice), g.Count()))
            .OrderByDescending(x => x.Revenue)
            .Take(10)
            .ToList();

        var periodDays = (end - start).Days;
        var prevStart = start.AddDays(-periodDays);
        var prevEnd = start;
        var prevRevenue = await salesDb.Sales
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= prevStart && s.SaleDate < prevEnd)
            .SumAsync(s => s.TotalAmount, cancellationToken);
        var prevPeriodComparison = prevRevenue > 0
            ? (double)((revenue - prevRevenue) / prevRevenue * 100)
            : (revenue > 0 ? 100.0 : 0.0);

        return new SalesReportResponse(
            revenue, salesCount, avgTicket,
            topBrands, topCategories, paymentBreakdown, dailySalesChart,
            new PreviousPeriodComparison(prevPeriodComparison, prevRevenue));
    }
}
