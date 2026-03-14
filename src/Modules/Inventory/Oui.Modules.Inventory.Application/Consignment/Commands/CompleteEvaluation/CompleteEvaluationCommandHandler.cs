using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;
using shs.Infrastructure.Services;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.CompleteEvaluation;

internal sealed class CompleteEvaluationCommandHandler(
    InventoryDbContext db,
    IEmailService emailService,
    ILogger<CompleteEvaluationCommandHandler> logger)
    : ICommandHandler<CompleteEvaluationCommand, CompleteEvaluationResponse>
{
    public async Task<Result<CompleteEvaluationResponse>> Handle(
        CompleteEvaluationCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<CompleteEvaluationResponse>(ConsignmentErrors.ReceptionNotFound);

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Result.Failure<CompleteEvaluationResponse>(ConsignmentErrors.AlreadyEvaluated);

        var evaluatedCount = reception.Items.Count;
        if (evaluatedCount == 0)
            return Result.Failure<CompleteEvaluationResponse>(ConsignmentErrors.NoItemsEvaluated);

        if (evaluatedCount < reception.ItemCount)
            return Result.Failure<CompleteEvaluationResponse>(
                ConsignmentErrors.ItemCountMismatch(reception.ItemCount - evaluatedCount, reception.ItemCount));

        foreach (var item in reception.Items.Where(i => !i.IsRejected))
        {
            item.Status = ItemStatus.AwaitingAcceptance;
        }

        reception.Status = ReceptionStatus.Evaluated;
        reception.EvaluatedAt = DateTime.UtcNow;
        reception.EvaluatedBy = "system";
        reception.UpdatedOn = DateTime.UtcNow;
        reception.UpdatedBy = "system";

        var approvalToken = new ReceptionApprovalTokenEntity
        {
            ExternalId = Guid.NewGuid(),
            ReceptionId = reception.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };
        db.ReceptionApprovalTokens.Add(approvalToken);

        await db.SaveChangesAsync(cancellationToken);

        var emailSent = false;
        try
        {
            var approvalUrl = $"{request.BaseUrl}/approval/{approvalToken.Token}";
            var emailData = BuildEvaluationEmailData(reception, approvalUrl);
            await emailService.SendEvaluationEmailAsync(emailData, cancellationToken);
            emailSent = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to auto-send evaluation email for reception {Id}", reception.ExternalId);
        }

        return new CompleteEvaluationResponse(
            "Avaliação concluída com sucesso.",
            reception.ItemCount,
            reception.Items.Count(i => !i.IsRejected),
            reception.Items.Count(i => i.IsRejected),
            emailSent);
    }

    private static EvaluationEmailData BuildEvaluationEmailData(ReceptionEntity reception, string? approvalUrl = null)
    {
        return new EvaluationEmailData
        {
            SupplierName = reception.Supplier.Name,
            SupplierEmail = reception.Supplier.Email,
            ReceptionDate = reception.ReceptionDate,
            ReceptionRef = reception.ExternalId.ToString()[..8].ToUpper(),
            ApprovalUrl = approvalUrl,
            AcceptedItems = reception.Items
                .Where(i => !i.IsRejected)
                .Select(i => new EvaluationEmailItem
                {
                    IdentificationNumber = i.IdentificationNumber,
                    Name = i.Name,
                    Brand = i.Brand.Name,
                    Size = i.Size,
                    Color = i.Color,
                    Condition = i.Condition.ToString(),
                    EvaluatedPrice = i.EvaluatedPrice,
                })
                .ToList(),
            RejectedItems = reception.Items
                .Where(i => i.IsRejected)
                .Select(i => new EvaluationEmailItem
                {
                    IdentificationNumber = i.IdentificationNumber,
                    Name = i.Name,
                    Brand = i.Brand.Name,
                    Size = i.Size,
                    Color = i.Color,
                    Condition = i.Condition.ToString(),
                    EvaluatedPrice = i.EvaluatedPrice,
                    RejectionReason = i.RejectionReason,
                })
                .ToList(),
        };
    }
}
