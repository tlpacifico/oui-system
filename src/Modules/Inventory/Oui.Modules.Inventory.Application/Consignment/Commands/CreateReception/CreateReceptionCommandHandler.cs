using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.CreateReception;

internal sealed class CreateReceptionCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateReceptionCommand, ReceptionDetailResponse>
{
    public async Task<Result<ReceptionDetailResponse>> Handle(
        CreateReceptionCommand request, CancellationToken cancellationToken)
    {
        if (!request.SupplierExternalId.HasValue)
            return Result.Failure<ReceptionDetailResponse>(ConsignmentErrors.SupplierNotFound);

        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == request.SupplierExternalId.Value, cancellationToken);

        if (supplier is null)
            return Result.Failure<ReceptionDetailResponse>(ConsignmentErrors.SupplierNotFound);

        var reception = new ReceptionEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = supplier.Id,
            ReceptionDate = DateTime.UtcNow,
            ItemCount = request.ItemCount,
            Status = ReceptionStatus.PendingEvaluation,
            Notes = request.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Receptions.Add(reception);
        await db.SaveChangesAsync(cancellationToken);

        return new ReceptionDetailResponse(
            reception.ExternalId,
            new ReceptionSupplierInfo(supplier.ExternalId, supplier.Name, supplier.Initial),
            reception.ReceptionDate,
            reception.ItemCount,
            reception.Status.ToString(),
            reception.Notes,
            0,
            0,
            0,
            reception.EvaluatedAt,
            reception.EvaluatedBy,
            reception.CreatedOn,
            reception.CreatedBy);
    }
}
