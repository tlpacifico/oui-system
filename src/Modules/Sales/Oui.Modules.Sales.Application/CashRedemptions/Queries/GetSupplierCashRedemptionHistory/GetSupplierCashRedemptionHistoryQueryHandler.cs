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

        var balance = await salesDb.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == request.SupplierId)
            .SumAsync(t => t.Amount, cancellationToken);

        return new SupplierCashRedemptionHistoryResponse(
            request.SupplierId, supplier.Name, balance,
            total, request.Page, request.PageSize,
            (int)Math.Ceiling(total / (double)request.PageSize),
            transactions);
    }
}
