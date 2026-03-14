using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.RemoveEvaluationItem;

internal sealed class RemoveEvaluationItemCommandHandler(InventoryDbContext db)
    : ICommandHandler<RemoveEvaluationItemCommand>
{
    public async Task<Result> Handle(
        RemoveEvaluationItemCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ExternalId == request.ReceptionExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure(ConsignmentErrors.ReceptionNotFound);

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Result.Failure(ConsignmentErrors.CannotRemoveFromEvaluated);

        var item = await db.Items
            .FirstOrDefaultAsync(i => i.ExternalId == request.ItemExternalId && i.ReceptionId == reception.Id, cancellationToken);

        if (item is null)
            return Result.Failure(ConsignmentErrors.ItemNotFoundInReception);

        item.IsDeleted = true;
        item.DeletedBy = "system";
        item.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
