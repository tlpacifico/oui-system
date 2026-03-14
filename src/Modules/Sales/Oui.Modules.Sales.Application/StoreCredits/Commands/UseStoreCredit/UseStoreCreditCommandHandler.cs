using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.UseStoreCredit;

internal sealed class UseStoreCreditCommandHandler(SalesDbContext salesDb)
    : ICommandHandler<UseStoreCreditCommand, UseStoreCreditResponse>
{
    public async Task<Result<UseStoreCreditResponse>> Handle(
        UseStoreCreditCommand request, CancellationToken cancellationToken)
    {
        var credit = await salesDb.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == request.ExternalId, cancellationToken);

        if (credit == null)
            return Result.Failure<UseStoreCreditResponse>(StoreCreditErrors.NotFound);

        if (credit.Status != StoreCreditStatus.Active)
            return Result.Failure<UseStoreCreditResponse>(StoreCreditErrors.NotActive);

        if (credit.ExpiresOn.HasValue && credit.ExpiresOn.Value < DateTime.UtcNow)
            return Result.Failure<UseStoreCreditResponse>(StoreCreditErrors.Expired);

        if (request.Amount <= 0)
            return Result.Failure<UseStoreCreditResponse>(StoreCreditErrors.AmountMustBePositive);

        if (request.Amount > credit.CurrentBalance)
            return Result.Failure<UseStoreCreditResponse>(StoreCreditErrors.InsufficientBalance(credit.CurrentBalance));

        var now = DateTime.UtcNow;

        credit.CurrentBalance -= request.Amount;
        credit.UpdatedOn = now;
        credit.UpdatedBy = request.UserEmail;

        if (credit.CurrentBalance == 0)
            credit.Status = StoreCreditStatus.FullyUsed;

        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = credit.Id,
            SaleId = request.SaleId,
            Amount = -request.Amount,
            BalanceAfter = credit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Use,
            TransactionDate = now,
            ProcessedBy = request.UserEmail,
            Notes = request.Notes ?? "Store credit used in purchase",
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.StoreCreditTransactions.Add(transaction);
        await salesDb.SaveChangesAsync(cancellationToken);

        return new UseStoreCreditResponse(credit.ExternalId, request.Amount, credit.CurrentBalance, credit.Status);
    }
}
