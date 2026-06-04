using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Commands.ProcessPayment;

internal sealed class ProcessPaymentCommandHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : ICommandHandler<ProcessPaymentCommand, ProcessPaymentResponse>
{
    public async Task<Result<ProcessPaymentResponse>> Handle(
        ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var settlement = await salesDb.Settlements
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.ExternalId == request.ExternalId, cancellationToken);

        if (settlement == null)
            return Result.Failure<ProcessPaymentResponse>(SettlementErrors.NotFound);

        if (settlement.Status == SettlementStatus.Paid)
            return Result.Failure<ProcessPaymentResponse>(SettlementErrors.AlreadyPaid);

        if (settlement.Status == SettlementStatus.Cancelled)
            return Result.Failure<ProcessPaymentResponse>(SettlementErrors.IsCancelled);

        var now = DateTime.UtcNow;

        if (settlement.StoreCreditAmount > 0)
        {
            var storeCredit = new StoreCreditEntity
            {
                ExternalId = Guid.NewGuid(),
                SupplierId = settlement.SupplierId,
                SourceSettlementId = settlement.Id,
                OriginalAmount = settlement.StoreCreditAmount,
                CurrentBalance = settlement.StoreCreditAmount,
                IssuedOn = now,
                IssuedBy = request.UserEmail,
                Status = StoreCreditStatus.Active,
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
                Notes = $"Issued from settlement {settlement.ExternalId}",
                CreatedOn = now,
                CreatedBy = request.UserEmail
            };

            salesDb.StoreCreditTransactions.Add(transaction);
            settlement.StoreCreditId = storeCredit.Id;
        }

        // O fornecedor recebe apenas o crédito em loja (saldo único).
        // O resgate em dinheiro converte esse crédito (PorcInDinheiro/PorcInLoja)
        // no ecrã de Créditos em Loja — não existe saldo paralelo em dinheiro.

        settlement.Status = SettlementStatus.Paid;
        settlement.PaidOn = now;
        settlement.PaidBy = request.UserEmail;
        settlement.UpdatedOn = now;
        settlement.UpdatedBy = request.UserEmail;

        var itemIds = await salesDb.SaleItems
            .Where(si => si.SettlementId == settlement.Id)
            .Select(si => si.ItemId)
            .ToListAsync(cancellationToken);

        var items = await inventoryDb.Items
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            item.Status = ItemStatus.Paid;
            item.UpdatedOn = now;
            item.UpdatedBy = request.UserEmail;
        }

        await salesDb.SaveChangesAsync(cancellationToken);
        await inventoryDb.SaveChangesAsync(cancellationToken);

        var messages = new List<string>();
        if (settlement.StoreCreditAmount > 0)
            messages.Add($"Crédito em loja emitido: {settlement.StoreCreditAmount:C} (resgatável em dinheiro até {settlement.CashRedemptionAmount:C})");

        return new ProcessPaymentResponse(
            settlement.ExternalId,
            settlement.Status,
            settlement.PaidOn,
            settlement.PaidBy,
            string.Join(". ", messages));
    }
}
