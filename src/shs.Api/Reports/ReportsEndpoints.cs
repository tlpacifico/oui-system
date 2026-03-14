using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using Oui.Modules.Sales.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;

namespace shs.Api.Reports;

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/sales", GetSalesReport).RequirePermission("reports.view");
        group.MapGet("/inventory", GetInventoryReport).RequirePermission("reports.view");
        group.MapGet("/suppliers", GetSuppliersReport).RequirePermission("reports.view");
        group.MapGet("/finance", GetFinanceReport).RequirePermission("reports.view");
    }

    private static async Task<IResult> GetSalesReport(
        SalesDbContext salesDb,
        InventoryDbContext inventoryDb,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] long? brandId,
        [FromQuery] long? categoryId,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var start = startDate ?? now.Date.AddMonths(-1);
        var end = (endDate ?? now.Date).AddDays(1);

        // Load sales with payments and sale items (same module)
        var sales = await salesDb.Sales
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= start && s.SaleDate < end)
            .ToListAsync(ct);

        // Collect all item IDs from sale items to query inventory module
        var allItemIds = sales.SelectMany(s => s.Items).Select(si => si.ItemId).Distinct().ToList();

        // Load item details from inventory module
        var itemDetails = await inventoryDb.Items
            .Where(i => allItemIds.Contains(i.Id))
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Select(i => new
            {
                i.Id,
                i.BrandId,
                BrandName = i.Brand!.Name,
                i.CategoryId,
                CategoryName = i.Category != null ? i.Category.Name : (string?)null,
                i.AcquisitionType,
                i.CommissionAmount
            })
            .ToListAsync(ct);

        var itemDetailsById = itemDetails.ToDictionary(i => i.Id);

        // Filter by brand/category using in-memory join
        var filteredSales = sales.AsEnumerable();
        if (brandId.HasValue)
            filteredSales = filteredSales.Where(s => s.Items.Any(si =>
                itemDetailsById.TryGetValue(si.ItemId, out var item) && item.BrandId == brandId.Value));
        if (categoryId.HasValue)
            filteredSales = filteredSales.Where(s => s.Items.Any(si =>
                itemDetailsById.TryGetValue(si.ItemId, out var item) && item.CategoryId == categoryId.Value));

        var salesList = filteredSales.ToList();
        var revenue = salesList.Sum(s => s.TotalAmount);
        var salesCount = salesList.Count;
        var avgTicket = salesCount > 0 ? revenue / salesCount : 0m;

        var paymentBreakdown = salesList
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(g => g.Key.ToString(), g => new { count = g.Count(), total = g.Sum(p => p.Amount) });

        var dailySalesChart = salesList
            .GroupBy(s => s.SaleDate.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), revenue = g.Sum(s => s.TotalAmount), count = g.Count() })
            .ToList();

        var topBrands = salesList
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.ContainsKey(si.ItemId))
            .GroupBy(si => new { itemDetailsById[si.ItemId].BrandId, itemDetailsById[si.ItemId].BrandName })
            .Select(g => new { g.Key.BrandName, revenue = g.Sum(si => si.FinalPrice), count = g.Count() })
            .OrderByDescending(x => x.revenue)
            .Take(10)
            .ToList();

        var topCategories = salesList
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.TryGetValue(si.ItemId, out var item) && item.CategoryId != null)
            .GroupBy(si => new { itemDetailsById[si.ItemId].CategoryId, itemDetailsById[si.ItemId].CategoryName })
            .Select(g => new { g.Key!.CategoryName, revenue = g.Sum(si => si.FinalPrice), count = g.Count() })
            .OrderByDescending(x => x.revenue)
            .Take(10)
            .ToList();

        var periodDays = (end - start).Days;
        var prevStart = start.AddDays(-periodDays);
        var prevEnd = start;
        var prevRevenue = await salesDb.Sales
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= prevStart && s.SaleDate < prevEnd)
            .SumAsync(s => s.TotalAmount, ct);
        var prevPeriodComparison = prevRevenue > 0
            ? (double)((revenue - prevRevenue) / prevRevenue * 100)
            : (revenue > 0 ? 100.0 : 0.0);

        return Results.Ok(new
        {
            revenue,
            salesCount,
            avgTicket,
            topBrands,
            topCategories,
            paymentBreakdown,
            dailySalesChart,
            previousPeriodComparison = new { percentChange = prevPeriodComparison, previousRevenue = prevRevenue }
        });
    }

    private static async Task<IResult> GetInventoryReport(
        InventoryDbContext db,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var toSellItems = await db.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.ToSell)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Select(i => new { i.Id, i.ExternalId, i.EvaluatedPrice, i.DaysInStock, i.CreatedOn, i.BrandId, i.CategoryId, BrandName = i.Brand!.Name, CategoryName = i.Category != null ? i.Category.Name : (string?)null })
            .ToListAsync(ct);

        var totalItems = toSellItems.Count;
        var totalValue = toSellItems.Sum(i => i.EvaluatedPrice);

        int GetDays(int daysInStock, DateTime createdOn) =>
            daysInStock > 0 ? daysInStock : (int)(now - createdOn).TotalDays;

        var agingDistribution = new
        {
            days0_15 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 0 && d < 15; }),
            days15_30 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 15 && d < 30; }),
            days30_45 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 30 && d < 45; }),
            days45_60 = toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 45 && d < 60; }),
            days60Plus = toSellItems.Count(i => GetDays(i.DaysInStock, i.CreatedOn) >= 60)
        };

        var soldCount = await db.Items.CountAsync(i => !i.IsDeleted && i.Status == ItemStatus.Sold, ct);
        var totalEverInStock = totalItems + soldCount;
        var sellThroughRate = totalEverInStock > 0 ? (double)soldCount / totalEverInStock * 100 : 0;

        var soldByBrand = await db.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold)
            .GroupBy(i => new { i.BrandId, BrandName = i.Brand!.Name })
            .Select(g => new { g.Key.BrandId, g.Key.BrandName, SoldCount = g.Count() })
            .ToListAsync(ct);

        var inStockByBrand = toSellItems
            .GroupBy(i => new { i.BrandId, i.BrandName })
            .Select(g => new { g.Key.BrandId, g.Key.BrandName, InStockCount = g.Count() })
            .ToList();

        var sellThroughByBrand = inStockByBrand
            .Select(b => new
            {
                b.BrandName,
                inStock = b.InStockCount,
                sold = soldByBrand.FirstOrDefault(s => s.BrandId == b.BrandId)?.SoldCount ?? 0
            })
            .Concat(soldByBrand
                .Where(s => !inStockByBrand.Any(b => b.BrandId == s.BrandId))
                .Select(s => new { BrandName = s.BrandName, inStock = 0, sold = s.SoldCount }))
            .Where(x => x.inStock + x.sold > 0)
            .Select(x => new
            {
                x.BrandName,
                x.inStock,
                x.sold,
                sellThroughRate = (x.inStock + x.sold) > 0 ? (double)x.sold / (x.inStock + x.sold) * 100 : 0
            })
            .OrderByDescending(x => x.sellThroughRate)
            .Take(10)
            .ToList();

        var stagnantItems = toSellItems
            .Where(i => GetDays(i.DaysInStock, i.CreatedOn) >= 30)
            .OrderByDescending(i => GetDays(i.DaysInStock, i.CreatedOn))
            .Take(20)
            .Select(i => new
            {
                i.Id,
                i.ExternalId,
                i.BrandName,
                i.CategoryName,
                i.EvaluatedPrice,
                DaysInStock = GetDays(i.DaysInStock, i.CreatedOn)
            })
            .ToList();

        return Results.Ok(new
        {
            totalItems,
            totalValue,
            agingDistribution,
            sellThroughRate,
            sellThroughByBrand,
            stagnantItemsList = stagnantItems
        });
    }

    private static async Task<IResult> GetSuppliersReport(
        SalesDbContext salesDb,
        InventoryDbContext inventoryDb,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var start = startDate ?? now.Date.AddMonths(-1);
        var end = (endDate ?? now.Date).AddDays(1);

        var suppliers = await inventoryDb.Suppliers
            .Where(s => !s.IsDeleted)
            .Select(s => new { s.Id, s.ExternalId, s.Name, s.Initial })
            .ToListAsync(ct);

        var soldItemsInPeriod = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold && i.SupplierId != null &&
                        i.SoldAt >= start && i.SoldAt < end)
            .Select(i => new { i.SupplierId, i.FinalSalePrice, i.SoldAt, i.CreatedOn, i.Id })
            .ToListAsync(ct);

        var returnedItems = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Returned && i.SupplierId != null &&
                        i.ReturnedAt >= start && i.ReturnedAt < end)
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        // Cross-module: get settled item IDs from sales module
        var settledItemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId != null)
            .Select(si => si.ItemId)
            .Distinct()
            .ToListAsync(ct);

        var pendingBySupplier = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold && i.AcquisitionType == AcquisitionType.Consignment && i.SupplierId != null)
            .Where(i => !settledItemIds.Contains(i.Id))
            .GroupBy(i => i.SupplierId)
            .Select(g => new { SupplierId = g.Key, PendingAmount = g.Sum(i => i.FinalSalePrice ?? 0) })
            .ToListAsync(ct);

        var ranking = suppliers.Select(s => new
        {
            s.Id,
            s.ExternalId,
            s.Name,
            s.Initial,
            Revenue = soldItemsInPeriod.Where(i => i.SupplierId == s.Id).Sum(i => i.FinalSalePrice ?? 0),
            SoldCount = soldItemsInPeriod.Count(i => i.SupplierId == s.Id),
            ReturnedCount = returnedItems.FirstOrDefault(r => r.SupplierId == s.Id)?.Count ?? 0,
            PendingAmount = pendingBySupplier.FirstOrDefault(p => p.SupplierId == s.Id)?.PendingAmount ?? 0
        })
        .Where(x => x.SoldCount > 0 || x.PendingAmount > 0)
        .OrderByDescending(x => x.Revenue)
        .Select(x => new
        {
            x.Id,
            x.ExternalId,
            x.Name,
            x.Initial,
            x.Revenue,
            x.SoldCount,
            x.ReturnedCount,
            ReturnRate = x.SoldCount > 0 ? (double)x.ReturnedCount / x.SoldCount * 100 : 0,
            x.PendingAmount,
            AvgDaysToSell = x.SoldCount > 0
                ? soldItemsInPeriod
                    .Where(i => i.SupplierId == x.Id && i.SoldAt.HasValue)
                    .Select(i => (i.SoldAt!.Value - i.CreatedOn).TotalDays)
                    .DefaultIfEmpty(0)
                    .Average()
                : 0.0
        })
        .ToList();

        return Results.Ok(new
        {
            period = new { start, end },
            ranking
        });
    }

    private static async Task<IResult> GetFinanceReport(
        SalesDbContext salesDb,
        InventoryDbContext inventoryDb,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var start = startDate ?? now.Date.AddMonths(-1);
        var end = (endDate ?? now.Date).AddDays(1);

        // Load sales with sale items (same module)
        var salesInPeriod = await salesDb.Sales
            .Where(s => !s.IsDeleted && s.Status == SaleStatus.Active && s.SaleDate >= start && s.SaleDate < end)
            .Include(s => s.Items)
            .ToListAsync(ct);

        var grossRevenue = salesInPeriod.Sum(s => s.TotalAmount);

        // Cross-module: get item details from inventory to calculate commission
        var allItemIds = salesInPeriod.SelectMany(s => s.Items).Select(si => si.ItemId).Distinct().ToList();
        var itemDetails = await inventoryDb.Items
            .Where(i => allItemIds.Contains(i.Id))
            .Select(i => new { i.Id, i.AcquisitionType, i.CommissionAmount })
            .ToListAsync(ct);
        var itemDetailsById = itemDetails.ToDictionary(i => i.Id);

        var commissionRevenue = salesInPeriod
            .SelectMany(s => s.Items)
            .Where(si => itemDetailsById.TryGetValue(si.ItemId, out var item) && item.AcquisitionType == AcquisitionType.Consignment)
            .Sum(si => itemDetailsById.TryGetValue(si.ItemId, out var item) ? item.CommissionAmount ?? 0 : 0);

        // Cross-module: pending settlements
        var settledItemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId != null)
            .Select(si => si.ItemId)
            .Distinct()
            .ToListAsync(ct);

        var pendingSettlements = await inventoryDb.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment && i.SupplierId != null)
            .Where(i => !settledItemIds.Contains(i.Id))
            .SumAsync(i => i.FinalSalePrice ?? 0, ct);

        var paidSettlements = await salesDb.Settlements
            .Where(s => !s.IsDeleted && s.Status == SettlementStatus.Paid && s.PaidOn >= start && s.PaidOn < end)
            .SumAsync(s => s.NetAmountToSupplier, ct);

        var projectedCashflow = grossRevenue - commissionRevenue - pendingSettlements;

        return Results.Ok(new
        {
            period = new { start, end },
            grossRevenue,
            commissionRevenue,
            pendingSettlements,
            paidSettlements,
            projectedCashflow
        });
    }
}
