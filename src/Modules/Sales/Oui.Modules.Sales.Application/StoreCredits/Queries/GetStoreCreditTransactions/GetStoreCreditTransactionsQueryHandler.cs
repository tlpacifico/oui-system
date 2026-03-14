using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetStoreCreditTransactions;

internal sealed class GetStoreCreditTransactionsQueryHandler(SalesDbContext salesDb)
    : IQueryHandler<GetStoreCreditTransactionsQuery, StoreCreditTransactionsResponse>
{
    public async Task<Result<StoreCreditTransactionsResponse>> Handle(
        GetStoreCreditTransactionsQuery request, CancellationToken cancellationToken)
    {
        var credit = await salesDb.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == request.ExternalId, cancellationToken);

        if (credit == null)
            return Result.Failure<StoreCreditTransactionsResponse>(StoreCreditErrors.NotFound);

        var transactions = await salesDb.StoreCreditTransactions
            .Include(t => t.Sale)
            .Where(t => t.StoreCreditId == credit.Id)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new StoreCreditTransactionItem(
                t.ExternalId, t.Amount, t.BalanceAfter, t.TransactionType,
                t.TransactionDate, t.ProcessedBy, t.Notes,
                t.Sale != null
                    ? new StoreCreditTransactionSale(t.Sale.ExternalId, t.Sale.SaleNumber, t.Sale.TotalAmount)
                    : null))
            .ToListAsync(cancellationToken);

        return new StoreCreditTransactionsResponse(request.ExternalId, credit.CurrentBalance, transactions);
    }
}
