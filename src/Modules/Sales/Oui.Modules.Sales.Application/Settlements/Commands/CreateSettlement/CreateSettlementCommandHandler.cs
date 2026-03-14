using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Commands.CreateSettlement;

internal sealed class CreateSettlementCommandHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : ICommandHandler<CreateSettlementCommand, CreateSettlementResponse>
{
    public async Task<Result<CreateSettlementResponse>> Handle(
        CreateSettlementCommand request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<CreateSettlementResponse>(SettlementErrors.SupplierNotFound);

        var periodStart = ToUtc(request.PeriodStart);
        var periodEnd = ToUtc(request.PeriodEnd).AddDays(1);

        var items = await inventoryDb.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId == request.SupplierId &&
                       i.UpdatedOn >= periodStart &&
                       i.UpdatedOn < periodEnd)
            .Select(i => new { i.Id, i.FinalSalePrice })
            .ToListAsync(cancellationToken);

        var itemIds = items.Select(i => i.Id).ToList();
        var alreadySettledIds = await salesDb.SaleItems
            .Where(si => itemIds.Contains(si.ItemId) && si.SettlementId != null)
            .Select(si => si.ItemId)
            .ToListAsync(cancellationToken);

        items = items.Where(i => !alreadySettledIds.Contains(i.Id)).ToList();

        if (items.Count == 0)
            return Result.Failure<CreateSettlementResponse>(SettlementErrors.NoUnsettledItems);

        var totalSalesAmount = items.Sum(i => i.FinalSalePrice ?? 0);
        var porcInLoja = supplier.CreditPercentageInStore / 100m;
        var porcInDinheiro = supplier.CashRedemptionPercentage / 100m;
        var storeCreditAmount = totalSalesAmount * porcInLoja;
        var cashRedemptionAmount = totalSalesAmount * porcInDinheiro;
        var netAmountToSupplier = storeCreditAmount + cashRedemptionAmount;
        var storeCommissionAmount = totalSalesAmount - netAmountToSupplier;

        var now = DateTime.UtcNow;
        var settlement = new SettlementEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            PeriodStart = periodStart,
            PeriodEnd = ToUtc(request.PeriodEnd),
            TotalSalesAmount = totalSalesAmount,
            CreditPercentageInStore = supplier.CreditPercentageInStore,
            CashRedemptionPercentage = supplier.CashRedemptionPercentage,
            StoreCreditAmount = storeCreditAmount,
            CashRedemptionAmount = cashRedemptionAmount,
            StoreCommissionAmount = storeCommissionAmount,
            NetAmountToSupplier = netAmountToSupplier,
            PaymentMethod = SettlementPaymentMethod.StoreCredit,
            Status = SettlementStatus.Pending,
            Notes = request.Notes,
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.Settlements.Add(settlement);
        await salesDb.SaveChangesAsync(cancellationToken);

        var saleItems = await salesDb.SaleItems
            .Where(si => itemIds.Contains(si.ItemId))
            .ToListAsync(cancellationToken);

        foreach (var saleItem in saleItems)
            saleItem.SettlementId = settlement.Id;

        await salesDb.SaveChangesAsync(cancellationToken);

        return new CreateSettlementResponse(
            settlement.ExternalId, settlement.SupplierId, supplier.Name,
            settlement.PeriodStart, settlement.PeriodEnd,
            settlement.TotalSalesAmount,
            settlement.CreditPercentageInStore, settlement.CashRedemptionPercentage,
            settlement.StoreCreditAmount, settlement.CashRedemptionAmount,
            settlement.StoreCommissionAmount, settlement.NetAmountToSupplier,
            settlement.Status, items.Count, settlement.Notes,
            settlement.CreatedOn, settlement.CreatedBy);
    }

    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}
