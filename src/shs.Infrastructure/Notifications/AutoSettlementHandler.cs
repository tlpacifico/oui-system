using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Notifications;
using shs.Infrastructure.Services;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;

namespace shs.Infrastructure.Notifications;

public class AutoSettlementHandler : ISaleNotificationHandler
{
    private readonly SalesDbContext _salesDb;
    private readonly InventoryDbContext _inventoryDb;
    private readonly SystemSettingService _settings;
    private readonly ILogger<AutoSettlementHandler> _logger;

    public AutoSettlementHandler(
        SalesDbContext salesDb,
        InventoryDbContext inventoryDb,
        SystemSettingService settings,
        ILogger<AutoSettlementHandler> logger)
    {
        _salesDb = salesDb;
        _inventoryDb = inventoryDb;
        _settings = settings;
        _logger = logger;
    }

    public async Task HandleAsync(SaleCompletedNotification notification, CancellationToken ct)
    {
        var autoCreate = await _settings.GetBool("pos.auto_create_settlement");
        if (!autoCreate)
            return;

        // Load sold consignment items from Inventory module
        var soldItems = await _inventoryDb.Items
            .Include(i => i.Supplier)
            .Where(i => notification.SoldItemIds.Contains(i.Id)
                        && i.AcquisitionType == AcquisitionType.Consignment
                        && i.SupplierId != null)
            .ToListAsync(ct);

        if (soldItems.Count == 0)
            return;

        // Filter out items already linked to a settlement (from Sales module)
        var soldItemIds = soldItems.Select(i => i.Id).ToList();
        var alreadySettledItemIds = await _salesDb.SaleItems
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
            // O fornecedor recebe UM saldo único em crédito de loja (PorcInLoja).
            // O valor em dinheiro é alternativo, não cumulativo: resgates convertem
            // o crédito à taxa PorcInDinheiro/PorcInLoja (ex.: 40/50 = 0.8).
            var storeCreditAmount = Math.Round(totalSalesAmount * porcInLoja, 2, MidpointRounding.AwayFromZero);
            var cashRedemptionAmount = Math.Round(totalSalesAmount * porcInDinheiro, 2, MidpointRounding.AwayFromZero);
            var netAmountToSupplier = storeCreditAmount;
            var storeCommissionAmount = totalSalesAmount - storeCreditAmount;

            // Acerto automático nasce já finalizado: o crédito em loja é emitido
            // de imediato para o fornecedor poder usar na próxima compra.
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
                Status = SettlementStatus.Paid,
                PaidOn = now,
                PaidBy = "system",
                Notes = $"Criado automaticamente a partir da venda #{notification.SaleId}",
                CreatedOn = now,
                CreatedBy = "system"
            };

            _salesDb.Settlements.Add(settlement);
            await _salesDb.SaveChangesAsync(ct);

            if (storeCreditAmount > 0)
            {
                var storeCredit = new StoreCreditEntity
                {
                    ExternalId = Guid.NewGuid(),
                    SupplierId = group.Key,
                    SourceSettlementId = settlement.Id,
                    OriginalAmount = storeCreditAmount,
                    CurrentBalance = storeCreditAmount,
                    IssuedOn = now,
                    IssuedBy = "system",
                    Status = StoreCreditStatus.Active,
                    CreatedOn = now,
                    CreatedBy = "system"
                };

                _salesDb.StoreCredits.Add(storeCredit);
                await _salesDb.SaveChangesAsync(ct);

                _salesDb.StoreCreditTransactions.Add(new StoreCreditTransactionEntity
                {
                    ExternalId = Guid.NewGuid(),
                    StoreCreditId = storeCredit.Id,
                    Amount = storeCredit.OriginalAmount,
                    BalanceAfter = storeCredit.CurrentBalance,
                    TransactionType = StoreCreditTransactionType.Issue,
                    TransactionDate = now,
                    ProcessedBy = "system",
                    Notes = $"Issued from settlement {settlement.ExternalId}",
                    CreatedOn = now,
                    CreatedBy = "system"
                });

                settlement.StoreCreditId = storeCredit.Id;
            }

            // Link sale items to this settlement
            var saleItems = await _salesDb.SaleItems
                .Where(si => itemIds.Contains(si.ItemId) && si.SettlementId == null)
                .ToListAsync(ct);

            foreach (var saleItem in saleItems)
            {
                saleItem.SettlementId = settlement.Id;
            }

            await _salesDb.SaveChangesAsync(ct);

            // Itens passam diretamente a "Pago" — o acerto já está liquidado
            foreach (var item in itemsInGroup)
            {
                item.Status = ItemStatus.Paid;
                item.UpdatedOn = now;
                item.UpdatedBy = "system";
            }

            await _inventoryDb.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Auto-settlement created as Paid for supplier {SupplierName} (ID: {SupplierId}), " +
                "SaleId: {SaleId}, Items: {ItemCount}, Total: {Total:F2}€, StoreCredit: {Credit:F2}€",
                supplier.Name, supplier.Id, notification.SaleId, itemsInGroup.Count, totalSalesAmount, storeCreditAmount);
        }
    }
}
