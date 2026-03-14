using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Approvals.Commands.RejectByToken;

internal sealed class RejectByTokenCommandHandler(InventoryDbContext db)
    : ICommandHandler<RejectByTokenCommand, RejectByTokenResponse>
{
    public async Task<Result<RejectByTokenResponse>> Handle(
        RejectByTokenCommand request, CancellationToken cancellationToken)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (approvalToken is null)
            return Result.Failure<RejectByTokenResponse>(ApprovalErrors.InvalidToken);

        if (approvalToken.IsUsed)
            return Result.Failure<RejectByTokenResponse>(ApprovalErrors.AlreadyProcessed);

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<RejectByTokenResponse>(ApprovalErrors.TokenExpired);

        approvalToken.IsUsed = true;
        approvalToken.ApprovedAt = DateTime.UtcNow;
        approvalToken.ApprovedBy = $"supplier (rejected: {request.Message?.Trim() ?? "sem motivo"})";
        approvalToken.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new RejectByTokenResponse(
            "A sua resposta foi registada. A equipa da loja entrará em contacto consigo.");
    }
}
