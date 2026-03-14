using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetInventoryReport;

internal sealed class GetInventoryReportQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetInventoryReportQuery, InventoryReportResponse>
{
    public async Task<Result<InventoryReportResponse>> Handle(
        GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var toSellItems = await db.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.ToSell)
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Select(i => new { i.Id, i.ExternalId, i.EvaluatedPrice, i.DaysInStock, i.CreatedOn, i.BrandId, i.CategoryId, BrandName = i.Brand!.Name, CategoryName = i.Category != null ? i.Category.Name : (string?)null })
            .ToListAsync(cancellationToken);

        var totalItems = toSellItems.Count;
        var totalValue = toSellItems.Sum(i => i.EvaluatedPrice);

        int GetDays(int daysInStock, DateTime createdOn) =>
            daysInStock > 0 ? daysInStock : (int)(now - createdOn).TotalDays;

        var agingDistribution = new AgingDistribution(
            toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 0 && d < 15; }),
            toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 15 && d < 30; }),
            toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 30 && d < 45; }),
            toSellItems.Count(i => { var d = GetDays(i.DaysInStock, i.CreatedOn); return d >= 45 && d < 60; }),
            toSellItems.Count(i => GetDays(i.DaysInStock, i.CreatedOn) >= 60));

        var soldCount = await db.Items.CountAsync(i => !i.IsDeleted && i.Status == ItemStatus.Sold, cancellationToken);
        var totalEverInStock = totalItems + soldCount;
        var sellThroughRate = totalEverInStock > 0 ? (double)soldCount / totalEverInStock * 100 : 0;

        var soldByBrand = await db.Items
            .Where(i => !i.IsDeleted && i.Status == ItemStatus.Sold)
            .GroupBy(i => new { i.BrandId, BrandName = i.Brand!.Name })
            .Select(g => new { g.Key.BrandId, g.Key.BrandName, SoldCount = g.Count() })
            .ToListAsync(cancellationToken);

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
            .Select(x => new SellThroughByBrandItem(
                x.BrandName,
                x.inStock,
                x.sold,
                (x.inStock + x.sold) > 0 ? (double)x.sold / (x.inStock + x.sold) * 100 : 0))
            .OrderByDescending(x => x.SellThroughRate)
            .Take(10)
            .ToList();

        var stagnantItems = toSellItems
            .Where(i => GetDays(i.DaysInStock, i.CreatedOn) >= 30)
            .OrderByDescending(i => GetDays(i.DaysInStock, i.CreatedOn))
            .Take(20)
            .Select(i => new StagnantItemInfo(
                i.Id, i.ExternalId, i.BrandName, i.CategoryName, i.EvaluatedPrice,
                GetDays(i.DaysInStock, i.CreatedOn)))
            .ToList();

        return new InventoryReportResponse(
            totalItems, totalValue, agingDistribution, sellThroughRate,
            sellThroughByBrand, stagnantItems);
    }
}
