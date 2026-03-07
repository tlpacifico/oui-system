using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Notifications;
using shs.Infrastructure.Database;
using shs.Infrastructure.Services;

namespace shs.Infrastructure.Notifications;

public class AutoSettlementHandler : ISaleNotificationHandler
{
    private readonly ShsDbContext _db;
    private readonly SystemSettingService _settings;
    private readonly ILogger<AutoSettlementHandler> _logger;

    public AutoSettlementHandler(
        ShsDbContext db,
        SystemSettingService settings,
        ILogger<AutoSettlementHandler> logger)
    {
        _db = db;
        _settings = settings;
        _logger = logger;
    }

    public async Task HandleAsync(SaleCompletedNotification notification, CancellationToken ct)
    {
        var autoCreate = await _settings.GetBool("pos.auto_create_settlement");
        if (!autoCreate)
            return;

        // Load sold consignment items with supplier info
        var soldItems = await _db.Items
            .Include(i => i.Supplier)
            .Where(i => notification.SoldItemIds.Contains(i.Id)
                        && i.AcquisitionType == AcquisitionType.Consignment
                        && i.SupplierId != null)
            .ToListAsync(ct);

        if (soldItems.Count == 0)
            return;

        // Filter out items already linked to a settlement
        var soldItemIds = soldItems.Select(i => i.Id).ToList();
        var alreadySettledItemIds = await _db.SaleItems
            .Where(si => soldItemIds.Contains(si.ItemId) && si.SettlementId != null)
            .Select(si => si.ItemId)
            .ToListAsync(ct);

        soldItems = soldItems.Where(i => !alreadySettledItemIds.Contains(i.Id)).ToList();

        if (soldItems.Count == 0)
            return;

        // Group by supplier
        var groupedBySupplier = soldItems.GroupBy(i => i.SupplierId!.Value);

        var saleDate = notification.SaleDate.Date;
        var saleDateUtc = saleDate.Kind == DateTimeKind.Utc
            ? saleDate
            : DateTime.SpecifyKind(saleDate, DateTimeKind.Utc);
        var now = DateTime.UtcNow;

        foreach (var group in groupedBySupplier)
        {
            var supplier = group.First().Supplier!;
            var itemsInGroup = group.ToList();
            var itemIds = itemsInGroup.Select(i => i.Id).ToList();

            var totalSalesAmount = itemsInGroup.Sum(i => i.FinalSalePrice ?? 0);
            var porcInLoja = supplier.CreditPercentageInStore / 100m;
            var porcInDinheiro = supplier.CashRedemptionPercentage / 100m;
            var storeCreditAmount = totalSalesAmount * porcInLoja;
            var cashRedemptionAmount = totalSalesAmount * porcInDinheiro;
            var netAmountToSupplier = storeCreditAmount + cashRedemptionAmount;
            var storeCommissionAmount = totalSalesAmount - netAmountToSupplier;

            var settlement = new SettlementEntity
            {
                ExternalId = Guid.NewGuid(),
                SupplierId = group.Key,
                PeriodStart = saleDateUtc,
                PeriodEnd = saleDateUtc,
                TotalSalesAmount = totalSalesAmount,
                CreditPercentageInStore = supplier.CreditPercentageInStore,
                CashRedemptionPercentage = supplier.CashRedemptionPercentage,
                StoreCreditAmount = storeCreditAmount,
                CashRedemptionAmount = cashRedemptionAmount,
                StoreCommissionAmount = storeCommissionAmount,
                NetAmountToSupplier = netAmountToSupplier,
                PaymentMethod = SettlementPaymentMethod.StoreCredit,
                Status = SettlementStatus.Pending,
                Notes = $"Criado automaticamente a partir da venda #{notification.SaleId}",
                CreatedOn = now,
                CreatedBy = "system"
            };

            _db.Settlements.Add(settlement);
            await _db.SaveChangesAsync(ct);

            // Link sale items to this settlement
            var saleItems = await _db.SaleItems
                .Where(si => itemIds.Contains(si.ItemId) && si.SettlementId == null)
                .ToListAsync(ct);

            foreach (var saleItem in saleItems)
            {
                saleItem.SettlementId = settlement.Id;
            }

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Auto-settlement created for supplier {SupplierName} (ID: {SupplierId}), " +
                "SaleId: {SaleId}, Items: {ItemCount}, Total: {Total:F2}€",
                supplier.Name, supplier.Id, notification.SaleId, itemsInGroup.Count, totalSalesAmount);
        }
    }
}
