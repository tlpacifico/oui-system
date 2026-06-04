using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashRedemptionHistory;

internal sealed class GetSupplierCashRedemptionHistoryQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetSupplierCashRedemptionHistoryQuery, SupplierCashRedemptionHistoryResponse>
{
    public async Task<Result<SupplierCashRedemptionHistoryResponse>> Handle(
        GetSupplierCashRedemptionHistoryQuery request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<SupplierCashRedemptionHistoryResponse>(CashRedemptionErrors.SupplierNotFound);

        var query = salesDb.SupplierCashBalanceTransactions
            .Include(t => t.Settlement)
            .Where(t => t.SupplierId == request.SupplierId)
            .OrderByDescending(t => t.TransactionDate);

        var total = await query.CountAsync(cancellationToken);

        var transactions = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new CashBalanceTransactionItem(
                t.ExternalId, t.Amount, t.TransactionType,
                t.TransactionDate, t.ProcessedBy, t.Notes,
                t.Settlement != null
                    ? $"{t.Settlement.PeriodStart:yyyy-MM-dd} - {t.Settlement.PeriodEnd:yyyy-MM-dd}"
                    : null))
            .ToListAsync(cancellationToken);

        // Saldo único: o valor resgatável deriva do crédito em loja ativo × taxa
        var creditBalance = await salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId && sc.Status == shs.Domain.Entities.StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .SumAsync(sc => sc.CurrentBalance, cancellationToken);

        var rate = supplier.CreditPercentageInStore > 0
            ? supplier.CashRedemptionPercentage / supplier.CreditPercentageInStore
            : 0m;
        var balance = Math.Floor(creditBalance * rate * 100m) / 100m;

        return new SupplierCashRedemptionHistoryResponse(
            request.SupplierId, supplier.Name, balance,
            total, request.Page, request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize),
            transactions);
    }
}
