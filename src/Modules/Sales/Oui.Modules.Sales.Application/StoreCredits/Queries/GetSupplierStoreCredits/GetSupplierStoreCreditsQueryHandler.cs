using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCredits;

internal sealed class GetSupplierStoreCreditsQueryHandler(SalesDbContext salesDb)
    : IQueryHandler<GetSupplierStoreCreditsQuery, SupplierStoreCreditsResponse>
{
    public async Task<Result<SupplierStoreCreditsResponse>> Handle(
        GetSupplierStoreCreditsQuery request, CancellationToken cancellationToken)
    {
        var query = salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId);

        if (request.Status.HasValue)
            query = query.Where(sc => sc.Status == request.Status.Value);

        var credits = await query
            .OrderByDescending(sc => sc.IssuedOn)
            .Select(sc => new StoreCreditListItem(
                sc.ExternalId, sc.OriginalAmount, sc.CurrentBalance, sc.Status,
                sc.IssuedOn, sc.IssuedBy, sc.ExpiresOn, sc.Notes,
                sc.SourceSettlement != null
                    ? new StoreCreditSourceSettlement(sc.SourceSettlement.ExternalId, sc.SourceSettlement.PeriodStart, sc.SourceSettlement.PeriodEnd)
                    : null))
            .ToListAsync(cancellationToken);

        var totalBalance = credits
            .Where(c => c.Status == StoreCreditStatus.Active)
            .Sum(c => c.CurrentBalance);

        return new SupplierStoreCreditsResponse(request.SupplierId, totalBalance, credits);
    }
}
