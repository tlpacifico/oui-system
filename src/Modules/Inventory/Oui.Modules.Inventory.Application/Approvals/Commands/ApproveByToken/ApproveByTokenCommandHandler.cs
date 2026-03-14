using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.ApproveByToken;

internal sealed class ApproveByTokenCommandHandler(InventoryDbContext db)
    : ICommandHandler<ApproveByTokenCommand, ApproveByTokenResponse>
{
    public async Task<Result<ApproveByTokenResponse>> Handle(
        ApproveByTokenCommand request, CancellationToken cancellationToken)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
                .ThenInclude(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (approvalToken is null)
            return Result.Failure<ApproveByTokenResponse>(ApprovalErrors.InvalidToken);

        if (approvalToken.IsUsed)
            return Result.Failure<ApproveByTokenResponse>(ApprovalErrors.AlreadyProcessed);

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<ApproveByTokenResponse>(ApprovalErrors.TokenExpired);

        var itemsApproved = 0;
        foreach (var item in approvalToken.Reception.Items.Where(i => i.Status == ItemStatus.AwaitingAcceptance))
        {
            item.Status = ItemStatus.ToSell;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "supplier";
            itemsApproved++;
        }

        approvalToken.IsUsed = true;
        approvalToken.ApprovedAt = DateTime.UtcNow;
        approvalToken.ApprovedBy = "supplier (via link)";
        approvalToken.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new ApproveByTokenResponse(
            "Aprovação registada com sucesso! As peças serão colocadas à venda.",
            itemsApproved);
    }
}
