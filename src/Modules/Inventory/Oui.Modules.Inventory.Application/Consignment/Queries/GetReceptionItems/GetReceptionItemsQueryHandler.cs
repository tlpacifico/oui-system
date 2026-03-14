using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionItems;

internal sealed class GetReceptionItemsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReceptionItemsQuery, List<EvaluationItemResponse>>
{
    public async Task<Result<List<EvaluationItemResponse>>> Handle(
        GetReceptionItemsQuery request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<List<EvaluationItemResponse>>(ConsignmentErrors.ReceptionNotFound);

        var items = await db.Items
            .Where(i => i.ReceptionId == reception.Id && !i.IsDeleted)
            .Include(i => i.Brand)
            .OrderBy(i => i.CreatedOn)
            .Select(i => new EvaluationItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.CommissionPercentage,
                i.Status.ToString(),
                i.IsRejected,
                i.RejectionReason,
                i.CreatedOn))
            .ToListAsync(cancellationToken);

        return items;
    }
}
