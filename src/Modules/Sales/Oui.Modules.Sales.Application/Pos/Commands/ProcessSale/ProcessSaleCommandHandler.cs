using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Notifications;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Commands.ProcessSale;

internal sealed class ProcessSaleCommandHandler(
    SalesDbContext salesDb,
    InventoryDbContext inventoryDb,
    EcommerceDbContext ecommerceDb,
    ISaleNotificationDispatcher dispatcher)
    : ICommandHandler<ProcessSaleCommand, ProcessSaleResponse>
{
    public async Task<Result<ProcessSaleResponse>> Handle(
        ProcessSaleCommand request, CancellationToken cancellationToken)
    {
        // ── Validate request ──
        if (request.Items is null || request.Items.Length == 0)
            return Result.Failure<ProcessSaleResponse>(PosErrors.NoItems);

        if (request.Payments is null || request.Payments.Length == 0)
            return Result.Failure<ProcessSaleResponse>(PosErrors.NoPayments);

        if (request.Payments.Length > 2)
            return Result.Failure<ProcessSaleResponse>(PosErrors.TooManyPayments);

        if (request.DiscountPercentage.HasValue && (request.DiscountPercentage < 0 || request.DiscountPercentage > 100))
            return Result.Failure<ProcessSaleResponse>(PosErrors.InvalidDiscountPercentage);

        // ── Validate cash register ──
        var register = await salesDb.CashRegisters
            .FirstOrDefaultAsync(r => r.ExternalId == request.CashRegisterId
                                      && r.Status == CashRegisterStatus.Open, cancellationToken);

        if (register is null)
            return Result.Failure<ProcessSaleResponse>(PosErrors.RegisterNotOpen);

        if (register.OperatorUserId != request.UserId)
            return Result.Failure<ProcessSaleResponse>(PosErrors.RegisterNotOwned);

        // ── Load and validate items ──
        var itemExternalIds = request.Items.Select(i => i.ItemExternalId).ToArray();

        var items = await inventoryDb.Items
            .Where(i => itemExternalIds.Contains(i.ExternalId) && !i.IsDeleted)
            .Include(i => i.Brand)
            .Include(i => i.Supplier)
            .ToListAsync(cancellationToken);

        if (items.Count != request.Items.Length)
        {
            var foundIds = items.Select(i => i.ExternalId).ToHashSet();
            var missing = request.Items.Where(ri => !foundIds.Contains(ri.ItemExternalId)).ToList();
            return Result.Failure<ProcessSaleResponse>(
                PosErrors.ItemsNotFound(string.Join(", ", missing.Select(m => m.ItemExternalId))));
        }

        var nonSellable = items.Where(i => i.Status != ItemStatus.ToSell).ToList();
        if (nonSellable.Count > 0)
        {
            var ids = string.Join(", ", nonSellable.Select(i => i.IdentificationNumber));
            return Result.Failure<ProcessSaleResponse>(PosErrors.ItemsNotSellable(ids));
        }

        // ── Calculate prices ──
        var saleItems = new List<SaleItemEntity>();
        decimal subtotal = 0;

        foreach (var reqItem in request.Items)
        {
            var item = items.First(i => i.ExternalId == reqItem.ItemExternalId);
            var unitPrice = item.EvaluatedPrice;
            var itemDiscount = reqItem.DiscountAmount;

            if (itemDiscount < 0)
                return Result.Failure<ProcessSaleResponse>(PosErrors.NegativeItemDiscount(item.IdentificationNumber));

            if (itemDiscount > unitPrice)
                return Result.Failure<ProcessSaleResponse>(PosErrors.ItemDiscountExceedsPrice(item.IdentificationNumber));

            var finalPrice = unitPrice - itemDiscount;

            saleItems.Add(new SaleItemEntity
            {
                ItemId = item.Id,
                UnitPrice = unitPrice,
                DiscountAmount = itemDiscount,
                FinalPrice = finalPrice,
            });

            subtotal += unitPrice;
        }

        var discountPercentage = request.DiscountPercentage ?? 0;
        var percentageDiscount = subtotal * discountPercentage / 100;
        var itemDiscountsTotal = saleItems.Sum(si => si.DiscountAmount);
        var totalDiscount = percentageDiscount + itemDiscountsTotal;
        var totalAmount = subtotal - totalDiscount;

        if (totalAmount < 0)
            return Result.Failure<ProcessSaleResponse>(PosErrors.NegativeTotalAmount);

        if (discountPercentage > 0)
        {
            foreach (var si in saleItems)
            {
                var proportionalDiscount = si.FinalPrice * discountPercentage / 100;
                si.FinalPrice -= proportionalDiscount;
                si.DiscountAmount += proportionalDiscount;
            }
        }

        // ── Validate payments ──
        var paymentTotal = request.Payments.Sum(p => p.Amount);
        if (paymentTotal < totalAmount)
            return Result.Failure<ProcessSaleResponse>(PosErrors.InsufficientPayment(paymentTotal, totalAmount));

        foreach (var p in request.Payments)
        {
            if (p.Amount <= 0)
                return Result.Failure<ProcessSaleResponse>(PosErrors.NonPositivePayment);

            if (!Enum.TryParse<PaymentMethodType>(p.Method, out _))
                return Result.Failure<ProcessSaleResponse>(PosErrors.InvalidPaymentMethod(p.Method));

            if (p.Method == nameof(PaymentMethodType.StoreCredit) && !p.SupplierId.HasValue)
                return Result.Failure<ProcessSaleResponse>(PosErrors.StoreCreditRequiresSupplier);
        }

        foreach (var p in request.Payments.Where(x => x.Method == nameof(PaymentMethodType.StoreCredit)))
        {
            if (!p.SupplierId.HasValue) continue;

            var creditBalance = await salesDb.StoreCredits
                .Where(sc => !sc.IsDeleted && sc.SupplierId == p.SupplierId && sc.Status == StoreCreditStatus.Active)
                .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
                .SumAsync(sc => sc.CurrentBalance, cancellationToken);

            if (creditBalance < p.Amount)
            {
                var supplier = await inventoryDb.Suppliers.FindAsync([p.SupplierId.Value], cancellationToken);
                var name = supplier?.Name ?? p.SupplierId.ToString()!;
                return Result.Failure<ProcessSaleResponse>(PosErrors.InsufficientStoreCredit(name, creditBalance));
            }
        }

        // ── Generate sale number ──
        var today = DateTime.UtcNow.Date;
        var todayPrefix = $"V{today:yyyyMMdd}-";

        var lastSaleNumber = await salesDb.Sales
            .IgnoreQueryFilters()
            .Where(s => s.SaleNumber.StartsWith(todayPrefix))
            .OrderByDescending(s => s.SaleNumber)
            .Select(s => s.SaleNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextSeq = 1;
        if (lastSaleNumber is not null)
        {
            var seqPart = lastSaleNumber[(todayPrefix.Length)..];
            if (int.TryParse(seqPart, out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        var saleNumber = $"{todayPrefix}{nextSeq:000}";

        // ── Create sale ──
        var sale = new SaleEntity
        {
            ExternalId = Guid.NewGuid(),
            SaleNumber = saleNumber,
            CashRegisterId = register.Id,
            SaleDate = DateTime.UtcNow,
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = totalDiscount,
            TotalAmount = totalAmount,
            DiscountReason = request.DiscountReason?.Trim(),
            Status = SaleStatus.Active,
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.UserId,
            Items = saleItems,
            Payments = request.Payments.Select(p => new SalePaymentEntity
            {
                PaymentMethod = Enum.Parse<PaymentMethodType>(p.Method),
                Amount = p.Amount,
                Reference = p.Reference?.Trim(),
                SupplierId = p.SupplierId,
            }).ToList()
        };

        salesDb.Sales.Add(sale);
        await salesDb.SaveChangesAsync(cancellationToken);

        // ── Deduct store credits ──
        foreach (var payment in sale.Payments.Where(p => p.PaymentMethod == PaymentMethodType.StoreCredit && p.SupplierId.HasValue))
        {
            var credits = await salesDb.StoreCredits
                .Where(sc => !sc.IsDeleted && sc.SupplierId == payment.SupplierId!.Value && sc.Status == StoreCreditStatus.Active)
                .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
                .OrderBy(sc => sc.IssuedOn)
                .ToListAsync(cancellationToken);

            var remainingToUse = payment.Amount;
            foreach (var credit in credits)
            {
                if (remainingToUse <= 0) break;
                var useAmount = Math.Min(credit.CurrentBalance, remainingToUse);
                if (useAmount <= 0) continue;

                credit.CurrentBalance -= useAmount;
                credit.UpdatedOn = DateTime.UtcNow;
                credit.UpdatedBy = request.UserId;
                if (credit.CurrentBalance == 0)
                    credit.Status = StoreCreditStatus.FullyUsed;

                var transaction = new StoreCreditTransactionEntity
                {
                    ExternalId = Guid.NewGuid(),
                    StoreCreditId = credit.Id,
                    SaleId = sale.Id,
                    Amount = -useAmount,
                    BalanceAfter = credit.CurrentBalance,
                    TransactionType = StoreCreditTransactionType.Use,
                    TransactionDate = DateTime.UtcNow,
                    ProcessedBy = request.UserId,
                    Notes = "Crédito usado em compra",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = request.UserId
                };
                salesDb.StoreCreditTransactions.Add(transaction);
                payment.StoreCreditId = credit.Id;
                remainingToUse -= useAmount;
            }
        }

        await salesDb.SaveChangesAsync(cancellationToken);

        // ── Update item statuses to Sold ──
        foreach (var item in items)
        {
            item.Status = ItemStatus.Sold;
            item.SaleId = sale.Id;
            item.SoldAt = DateTime.UtcNow;
            item.FinalSalePrice = saleItems.First(si => si.ItemId == item.Id).FinalPrice;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = request.UserId;

            if (item.AcquisitionType == AcquisitionType.Consignment && item.SupplierId.HasValue && item.Supplier != null)
            {
                var finalSalePrice = item.FinalSalePrice.Value;
                var porcInLoja = item.Supplier.CreditPercentageInStore / 100m;
                var porcInDinheiro = item.Supplier.CashRedemptionPercentage / 100m;
                item.CommissionAmount = finalSalePrice * (porcInLoja + porcInDinheiro);
                item.CommissionPercentage = item.Supplier.CreditPercentageInStore + item.Supplier.CashRedemptionPercentage;
            }
        }

        // Unpublish ecommerce products for sold items
        var soldItemDbIds = items.Select(i => i.Id).ToList();
        var ecommerceProducts = await ecommerceDb.EcommerceProducts
            .Where(p => soldItemDbIds.Contains(p.ItemId) &&
                (p.Status == EcommerceProductStatus.Published || p.Status == EcommerceProductStatus.Reserved))
            .ToListAsync(cancellationToken);

        foreach (var ep in ecommerceProducts)
        {
            ep.Status = EcommerceProductStatus.Sold;
            ep.UnpublishedAt = DateTime.UtcNow;
        }

        if (ecommerceProducts.Count > 0)
        {
            var reservedProductIds = ecommerceProducts
                .Where(p => p.Status == EcommerceProductStatus.Sold)
                .Select(p => p.Id)
                .ToList();

            if (reservedProductIds.Count > 0)
            {
                var affectedOrderIds = await ecommerceDb.EcommerceOrderItems
                    .Where(oi => reservedProductIds.Contains(oi.ProductId))
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                var ordersToCancel = await ecommerceDb.EcommerceOrders
                    .Where(o => affectedOrderIds.Contains(o.Id) &&
                        (o.Status == EcommerceOrderStatus.Pending || o.Status == EcommerceOrderStatus.Confirmed))
                    .ToListAsync(cancellationToken);

                foreach (var order in ordersToCancel)
                {
                    order.Status = EcommerceOrderStatus.Cancelled;
                    order.CancelledAt = DateTime.UtcNow;
                    order.CancellationReason = "Item vendido na loja física";
                }
            }
        }

        await inventoryDb.SaveChangesAsync(cancellationToken);
        await ecommerceDb.SaveChangesAsync(cancellationToken);

        // Dispatch post-sale notification
        var soldItemIds = items.Select(i => i.Id).ToArray();
        await dispatcher.DispatchSaleCompletedAsync(
            new SaleCompletedNotification(sale.Id, sale.SaleDate, soldItemIds), cancellationToken);

        var change = paymentTotal > totalAmount ? paymentTotal - totalAmount : 0;

        return new ProcessSaleResponse(
            sale.ExternalId,
            sale.SaleNumber,
            sale.SaleDate,
            sale.Subtotal,
            sale.DiscountPercentage,
            sale.DiscountAmount,
            sale.TotalAmount,
            change,
            sale.Items.Count,
            register.OperatorName);
    }
}
