using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.StaffApprove;

internal sealed class StaffApproveCommandHandler(InventoryDbContext db)
    : ICommandHandler<StaffApproveCommand, StaffApproveResponse>
{
    public async Task<Result<StaffApproveResponse>> Handle(
        StaffApproveCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<StaffApproveResponse>(ApprovalErrors.ReceptionNotFound);

        if (reception.Status != ReceptionStatus.Evaluated)
            return Result.Failure<StaffApproveResponse>(ApprovalErrors.NotInEvaluatedState);

        var awaitingItems = reception.Items.Where(i => i.Status == ItemStatus.AwaitingAcceptance).ToList();
        if (awaitingItems.Count == 0)
            return Result.Failure<StaffApproveResponse>(ApprovalErrors.NoAwaitingItems);

        foreach (var item in awaitingItems)
        {
            item.Status = ItemStatus.ToSell;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "system";
        }

        var unusedTokens = await db.ReceptionApprovalTokens
            .Where(t => t.ReceptionId == reception.Id && !t.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in unusedTokens)
        {
            token.IsUsed = true;
            token.ApprovedAt = DateTime.UtcNow;
            token.ApprovedBy = "staff";
            token.UpdatedOn = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new StaffApproveResponse(
            "Aprovação registada com sucesso.",
            awaitingItems.Count);
    }
}
