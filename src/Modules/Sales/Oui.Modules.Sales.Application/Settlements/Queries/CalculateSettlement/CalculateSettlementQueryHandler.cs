using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Queries.CalculateSettlement;

internal sealed class CalculateSettlementQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<CalculateSettlementQuery, CalculateSettlementResponse>
{
    public async Task<Result<CalculateSettlementResponse>> Handle(
        CalculateSettlementQuery request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<CalculateSettlementResponse>(SettlementErrors.SupplierNotFound);

        var periodStart = ToUtc(request.PeriodStart);
        var periodEnd = ToUtc(request.PeriodEnd).AddDays(1);

        var candidateItems = await inventoryDb.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId == request.SupplierId &&
                       i.UpdatedOn >= periodStart &&
                       i.UpdatedOn < periodEnd)
            .Select(i => new { i.Id, i.FinalSalePrice })
            .ToListAsync(cancellationToken);

        var candidateItemIds = candidateItems.Select(i => i.Id).ToList();
        var settledItemIds = await salesDb.SaleItems
            .Where(si => candidateItemIds.Contains(si.ItemId) && si.SettlementId != null)
            .Select(si => si.ItemId)
            .ToListAsync(cancellationToken);

        var items = candidateItems.Where(i => !settledItemIds.Contains(i.Id)).ToList();

        if (items.Count == 0)
            return Result.Failure<CalculateSettlementResponse>(SettlementErrors.NoItemsForSettlement);

        var totalSalesAmount = items.Sum(i => i.FinalSalePrice ?? 0);
        var porcInLoja = supplier.CreditPercentageInStore / 100m;
        var porcInDinheiro = supplier.CashRedemptionPercentage / 100m;
        var storeCreditAmount = totalSalesAmount * porcInLoja;
        var cashRedemptionAmount = totalSalesAmount * porcInDinheiro;
        var netAmountToSupplier = storeCreditAmount + cashRedemptionAmount;
        var storeCommissionAmount = totalSalesAmount - netAmountToSupplier;

        return new CalculateSettlementResponse(
            request.SupplierId, supplier.Name,
            request.PeriodStart, request.PeriodEnd,
            items.Count, totalSalesAmount,
            supplier.CreditPercentageInStore, supplier.CashRedemptionPercentage,
            storeCreditAmount, cashRedemptionAmount, netAmountToSupplier, storeCommissionAmount);
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
