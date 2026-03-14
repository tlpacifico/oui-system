using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;
using shs.Infrastructure.Services;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.SendEvaluationEmail;

internal sealed class SendEvaluationEmailCommandHandler(InventoryDbContext db, IEmailService emailService)
    : ICommandHandler<SendEvaluationEmailCommand, SendEvaluationEmailResponse>
{
    public async Task<Result<SendEvaluationEmailResponse>> Handle(
        SendEvaluationEmailCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<SendEvaluationEmailResponse>(ConsignmentErrors.ReceptionNotFound);

        if (reception.Status == ReceptionStatus.PendingEvaluation)
            return Result.Failure<SendEvaluationEmailResponse>(ConsignmentErrors.EvaluationNotCompleted);

        var latestToken = await db.ReceptionApprovalTokens
            .Where(t => t.ReceptionId == reception.Id && !t.IsUsed)
            .OrderByDescending(t => t.CreatedOn)
            .FirstOrDefaultAsync(cancellationToken);

        try
        {
            string? approvalUrl = null;
            if (latestToken != null)
            {
                approvalUrl = $"{request.BaseUrl}/approval/{latestToken.Token}";
            }
            var emailData = BuildEvaluationEmailData(reception, approvalUrl);
            await emailService.SendEvaluationEmailAsync(emailData, cancellationToken);

            return new SendEvaluationEmailResponse(
                "Email de avaliação enviado com sucesso.",
                reception.Supplier.Email);
        }
        catch (Exception)
        {
            return Result.Failure<SendEvaluationEmailResponse>(ConsignmentErrors.EmailSendFailed);
        }
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
