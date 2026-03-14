using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionById;

internal sealed class GetReceptionByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetReceptionByIdQuery, ReceptionDetailResponse>
{
    public async Task<Result<ReceptionDetailResponse>> Handle(
        GetReceptionByIdQuery request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<ReceptionDetailResponse>(ConsignmentErrors.ReceptionNotFound);

        return new ReceptionDetailResponse(
            reception.ExternalId,
            new ReceptionSupplierInfo(reception.Supplier.ExternalId, reception.Supplier.Name, reception.Supplier.Initial),
            reception.ReceptionDate,
            reception.ItemCount,
            reception.Status.ToString(),
            reception.Notes,
            reception.Items.Count,
            reception.Items.Count(i => !i.IsRejected && i.Status != ItemStatus.Rejected),
            reception.Items.Count(i => i.IsRejected),
            reception.EvaluatedAt,
            reception.EvaluatedBy,
            reception.CreatedOn,
            reception.CreatedBy);
    }
}
