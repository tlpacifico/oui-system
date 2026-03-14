using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.IssueStoreCredit;

internal sealed class IssueStoreCreditCommandHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : ICommandHandler<IssueStoreCreditCommand, IssueStoreCreditResponse>
{
    public async Task<Result<IssueStoreCreditResponse>> Handle(
        IssueStoreCreditCommand request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<IssueStoreCreditResponse>(StoreCreditErrors.SupplierNotFound);

        if (request.Amount <= 0)
            return Result.Failure<IssueStoreCreditResponse>(StoreCreditErrors.AmountMustBePositive);

        var now = DateTime.UtcNow;

        var storeCredit = new StoreCreditEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            OriginalAmount = request.Amount,
            CurrentBalance = request.Amount,
            IssuedOn = now,
            IssuedBy = request.UserEmail,
            ExpiresOn = request.ExpiresOn,
            Status = StoreCreditStatus.Active,
            Notes = request.Notes,
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.StoreCredits.Add(storeCredit);
        await salesDb.SaveChangesAsync(cancellationToken);

        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = storeCredit.Id,
            Amount = storeCredit.OriginalAmount,
            BalanceAfter = storeCredit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Issue,
            TransactionDate = now,
            ProcessedBy = request.UserEmail,
            Notes = request.Notes ?? "Manual store credit issuance",
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.StoreCreditTransactions.Add(transaction);
        await salesDb.SaveChangesAsync(cancellationToken);

        return new IssueStoreCreditResponse(
            storeCredit.ExternalId, storeCredit.SupplierId, supplier.Name,
            storeCredit.OriginalAmount, storeCredit.CurrentBalance,
            storeCredit.Status, storeCredit.IssuedOn, storeCredit.IssuedBy);
    }
}
