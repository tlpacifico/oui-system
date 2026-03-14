using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Approvals.Queries.GetApprovalDetails;

internal sealed class GetApprovalDetailsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetApprovalDetailsQuery, ApprovalDetailsResponse>
{
    public async Task<Result<ApprovalDetailsResponse>> Handle(
        GetApprovalDetailsQuery request, CancellationToken cancellationToken)
    {
        var approvalToken = await db.ReceptionApprovalTokens
            .Include(t => t.Reception)
                .ThenInclude(r => r.Supplier)
            .Include(t => t.Reception)
                .ThenInclude(r => r.Items.Where(i => !i.IsDeleted && !i.IsRejected))
                    .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken);

        if (approvalToken is null)
            return Result.Failure<ApprovalDetailsResponse>(ApprovalErrors.InvalidToken);

        if (approvalToken.IsUsed)
            return Result.Failure<ApprovalDetailsResponse>(ApprovalErrors.AlreadyProcessed);

        if (approvalToken.ExpiresAt < DateTime.UtcNow)
            return Result.Failure<ApprovalDetailsResponse>(ApprovalErrors.TokenExpired);

        var reception = approvalToken.Reception;
        var items = reception.Items
            .Where(i => i.Status == ItemStatus.AwaitingAcceptance)
            .Select(i => new ApprovalItemResponse(
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.CommissionPercentage))
            .ToList();

        return new ApprovalDetailsResponse(
            reception.Supplier.Name,
            reception.ReceptionDate,
            reception.ExternalId.ToString()[..8].ToUpper(),
            items,
            items.Sum(i => i.EvaluatedPrice),
            approvalToken.ExpiresAt);
    }
}
