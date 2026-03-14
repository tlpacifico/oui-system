using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.AdjustStoreCredit;

internal sealed class AdjustStoreCreditCommandHandler(SalesDbContext salesDb)
    : ICommandHandler<AdjustStoreCreditCommand, AdjustStoreCreditResponse>
{
    public async Task<Result<AdjustStoreCreditResponse>> Handle(
        AdjustStoreCreditCommand request, CancellationToken cancellationToken)
    {
        var credit = await salesDb.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == request.ExternalId, cancellationToken);

        if (credit == null)
            return Result.Failure<AdjustStoreCreditResponse>(StoreCreditErrors.NotFound);

        if (credit.Status != StoreCreditStatus.Active)
            return Result.Failure<AdjustStoreCreditResponse>(StoreCreditErrors.NotActive);

        var now = DateTime.UtcNow;
        var oldBalance = credit.CurrentBalance;

        credit.CurrentBalance += request.AdjustmentAmount;

        if (credit.CurrentBalance < 0)
            return Result.Failure<AdjustStoreCreditResponse>(StoreCreditErrors.NegativeBalanceAfterAdjustment);

        credit.UpdatedOn = now;
        credit.UpdatedBy = request.UserEmail;

        if (credit.CurrentBalance == 0)
            credit.Status = StoreCreditStatus.FullyUsed;

        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = credit.Id,
            Amount = request.AdjustmentAmount,
            BalanceAfter = credit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Adjustment,
            TransactionDate = now,
            ProcessedBy = request.UserEmail,
            Notes = request.Reason ?? "Manual adjustment",
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.StoreCreditTransactions.Add(transaction);
        await salesDb.SaveChangesAsync(cancellationToken);

        return new AdjustStoreCreditResponse(
            credit.ExternalId, oldBalance, request.AdjustmentAmount,
            credit.CurrentBalance, credit.Status, request.Reason);
    }
}
