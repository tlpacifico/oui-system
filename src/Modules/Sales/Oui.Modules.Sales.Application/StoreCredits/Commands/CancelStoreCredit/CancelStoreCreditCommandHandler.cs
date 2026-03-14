using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.CancelStoreCredit;

internal sealed class CancelStoreCreditCommandHandler(SalesDbContext salesDb)
    : ICommandHandler<CancelStoreCreditCommand, CancelStoreCreditResponse>
{
    public async Task<Result<CancelStoreCreditResponse>> Handle(
        CancelStoreCreditCommand request, CancellationToken cancellationToken)
    {
        var credit = await salesDb.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == request.ExternalId, cancellationToken);

        if (credit == null)
            return Result.Failure<CancelStoreCreditResponse>(StoreCreditErrors.NotFound);

        if (credit.Status == StoreCreditStatus.Cancelled)
            return Result.Failure<CancelStoreCreditResponse>(StoreCreditErrors.AlreadyCancelled);

        var now = DateTime.UtcNow;
        var remainingBalance = credit.CurrentBalance;

        credit.CurrentBalance = 0;
        credit.Status = StoreCreditStatus.Cancelled;
        credit.UpdatedOn = now;
        credit.UpdatedBy = request.UserEmail;

        if (remainingBalance > 0)
        {
            var transaction = new StoreCreditTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                StoreCreditId = credit.Id,
                Amount = -remainingBalance,
                BalanceAfter = 0,
                TransactionType = StoreCreditTransactionType.Cancellation,
                TransactionDate = now,
                ProcessedBy = request.UserEmail,
                Notes = request.Reason ?? "Store credit cancelled",
                CreatedOn = now,
                CreatedBy = request.UserEmail
            };

            salesDb.StoreCreditTransactions.Add(transaction);
        }

        await salesDb.SaveChangesAsync(cancellationToken);

        return new CancelStoreCreditResponse(
            credit.ExternalId, credit.Status, remainingBalance, "Store credit cancelled successfully.");
    }
}
