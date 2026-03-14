using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.UseStoreCreditBySupplier;

internal sealed class UseStoreCreditBySupplierCommandHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : ICommandHandler<UseStoreCreditBySupplierCommand, UseStoreCreditBySupplierResponse>
{
    public async Task<Result<UseStoreCreditBySupplierResponse>> Handle(
        UseStoreCreditBySupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<UseStoreCreditBySupplierResponse>(StoreCreditErrors.SupplierNotFound);

        var credits = await salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId && sc.Status == StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .OrderBy(sc => sc.IssuedOn)
            .ToListAsync(cancellationToken);

        var totalAvailable = credits.Sum(c => c.CurrentBalance);
        if (request.Amount <= 0)
            return Result.Failure<UseStoreCreditBySupplierResponse>(StoreCreditErrors.AmountMustBePositive);
        if (request.Amount > totalAvailable)
            return Result.Failure<UseStoreCreditBySupplierResponse>(StoreCreditErrors.InsufficientSupplierBalance(totalAvailable));

        var remainingToUse = request.Amount;
        var usedCredits = new List<UsedCreditInfo>();
        var now = DateTime.UtcNow;

        foreach (var credit in credits)
        {
            if (remainingToUse <= 0) break;

            var useAmount = Math.Min(credit.CurrentBalance, remainingToUse);
            if (useAmount <= 0) continue;

            credit.CurrentBalance -= useAmount;
            credit.UpdatedOn = now;
            credit.UpdatedBy = request.UserEmail;
            if (credit.CurrentBalance == 0)
                credit.Status = StoreCreditStatus.FullyUsed;

            var transaction = new StoreCreditTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                StoreCreditId = credit.Id,
                SaleId = request.SaleId,
                Amount = -useAmount,
                BalanceAfter = credit.CurrentBalance,
                TransactionType = StoreCreditTransactionType.Use,
                TransactionDate = now,
                ProcessedBy = request.UserEmail,
                Notes = request.Notes ?? "Crédito usado em compra",
                CreatedOn = now,
                CreatedBy = request.UserEmail
            };
            salesDb.StoreCreditTransactions.Add(transaction);

            usedCredits.Add(new UsedCreditInfo(credit.ExternalId, useAmount, credit.CurrentBalance));
            remainingToUse -= useAmount;
        }

        await salesDb.SaveChangesAsync(cancellationToken);

        return new UseStoreCreditBySupplierResponse(request.SupplierId, supplier.Name, request.Amount, usedCredits);
    }
}
