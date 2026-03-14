using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetSettlementById;

internal sealed class GetSettlementByIdQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSettlementByIdQuery, SettlementDetailResponse>
{
    public async Task<Result<SettlementDetailResponse>> Handle(
        GetSettlementByIdQuery request, CancellationToken cancellationToken)
    {
        var settlementData = await salesDb.Settlements
            .Include(s => s.SaleItems)
            .Include(s => s.StoreCredit)
            .Where(s => !s.IsDeleted && s.ExternalId == request.ExternalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (settlementData == null)
            return Result.Failure<SettlementDetailResponse>(SettlementErrors.NotFound);

        var supplier = await inventoryDb.Suppliers
            .Where(s => s.Id == settlementData.SupplierId)
            .Select(s => new { s.Name, s.Email, s.PhoneNumber })
            .FirstOrDefaultAsync(cancellationToken);

        var itemIds = settlementData.SaleItems.Select(si => si.ItemId).ToList();
        var inventoryItems = await inventoryDb.Items
            .Include(i => i.Brand)
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id, cancellationToken);

        var storeCreditInfo = settlementData.StoreCredit != null
            ? new SettlementStoreCreditInfo(
                settlementData.StoreCredit.ExternalId,
                settlementData.StoreCredit.OriginalAmount,
                settlementData.StoreCredit.CurrentBalance,
                settlementData.StoreCredit.Status,
                settlementData.StoreCredit.IssuedOn)
            : null;

        var itemsList = settlementData.SaleItems.Select(si =>
        {
            var item = inventoryItems.GetValueOrDefault(si.ItemId);
            return new SettlementItemInfo(
                item?.ExternalId ?? Guid.Empty,
                item?.IdentificationNumber ?? "",
                item?.Name ?? "",
                item?.Brand?.Name ?? "",
                item?.EvaluatedPrice ?? 0m,
                si.FinalPrice,
                item?.UpdatedOn);
        }).ToList();

        return new SettlementDetailResponse(
            settlementData.ExternalId, settlementData.SupplierId,
            supplier?.Name ?? "", supplier?.Email, supplier?.PhoneNumber,
            settlementData.PeriodStart, settlementData.PeriodEnd,
            settlementData.TotalSalesAmount,
            settlementData.CreditPercentageInStore, settlementData.CashRedemptionPercentage,
            settlementData.StoreCreditAmount, settlementData.CashRedemptionAmount,
            settlementData.StoreCommissionAmount, settlementData.NetAmountToSupplier,
            settlementData.Status, settlementData.PaidOn, settlementData.PaidBy,
            settlementData.Notes, settlementData.CreatedOn, settlementData.CreatedBy,
            storeCreditInfo, itemsList);
    }
}
