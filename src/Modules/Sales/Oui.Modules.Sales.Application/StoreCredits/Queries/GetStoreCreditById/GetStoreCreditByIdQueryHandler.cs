using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetStoreCreditById;

internal sealed class GetStoreCreditByIdQueryHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : IQueryHandler<GetStoreCreditByIdQuery, StoreCreditDetailResponse>
{
    public async Task<Result<StoreCreditDetailResponse>> Handle(
        GetStoreCreditByIdQuery request, CancellationToken cancellationToken)
    {
        var creditData = await salesDb.StoreCredits
            .Include(sc => sc.SourceSettlement)
            .Include(sc => sc.Transactions)
            .Where(sc => !sc.IsDeleted && sc.ExternalId == request.ExternalId)
            .FirstOrDefaultAsync(cancellationToken);

        if (creditData == null)
            return Result.Failure<StoreCreditDetailResponse>(StoreCreditErrors.NotFound);

        var supplier = await inventoryDb.Suppliers
            .Where(s => s.Id == creditData.SupplierId)
            .Select(s => new { s.Name })
            .FirstOrDefaultAsync(cancellationToken);

        var lastTransaction = creditData.Transactions
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new StoreCreditLastTransaction(t.TransactionDate, t.Amount, t.TransactionType))
            .FirstOrDefault();

        var sourceSettlement = creditData.SourceSettlement != null
            ? new StoreCreditDetailSettlement(
                creditData.SourceSettlement.ExternalId,
                creditData.SourceSettlement.TotalSalesAmount,
                creditData.SourceSettlement.PeriodStart,
                creditData.SourceSettlement.PeriodEnd)
            : null;

        return new StoreCreditDetailResponse(
            creditData.ExternalId, creditData.SupplierId,
            supplier?.Name ?? "",
            creditData.OriginalAmount, creditData.CurrentBalance, creditData.Status,
            creditData.IssuedOn, creditData.IssuedBy, creditData.ExpiresOn, creditData.Notes,
            sourceSettlement, creditData.Transactions.Count, lastTransaction);
    }
}
