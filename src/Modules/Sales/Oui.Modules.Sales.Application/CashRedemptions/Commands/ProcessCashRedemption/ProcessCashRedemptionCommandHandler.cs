using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.CashRedemptions.Commands.ProcessCashRedemption;

internal sealed class ProcessCashRedemptionCommandHandler(SalesDbContext salesDb, InventoryDbContext inventoryDb)
    : ICommandHandler<ProcessCashRedemptionCommand, ProcessCashRedemptionResponse>
{
    public async Task<Result<ProcessCashRedemptionResponse>> Handle(
        ProcessCashRedemptionCommand request, CancellationToken cancellationToken)
    {
        var supplier = await inventoryDb.Suppliers.FindAsync([request.SupplierId], cancellationToken);
        if (supplier == null)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.SupplierNotFound);

        if (request.Amount <= 0)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.AmountMustBePositive);

        if (supplier.CreditPercentageInStore <= 0 || supplier.CashRedemptionPercentage <= 0)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.InvalidConversionRates);

        // O resgate em dinheiro converte o crédito em loja à taxa
        // PorcInDinheiro/PorcInLoja (ex.: 40/50 = 0.8 → €7.00 crédito = €5.60 dinheiro).
        var rate = supplier.CashRedemptionPercentage / supplier.CreditPercentageInStore;

        var credits = await salesDb.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == request.SupplierId && sc.Status == StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .OrderBy(sc => sc.IssuedOn)
            .ToListAsync(cancellationToken);

        var creditBalance = credits.Sum(c => c.CurrentBalance);
        var maxCash = Math.Floor(creditBalance * rate * 100m) / 100m;

        if (request.Amount > maxCash)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.InsufficientBalance(maxCash));

        var creditToDebit = Math.Round(request.Amount / rate, 2, MidpointRounding.AwayFromZero);
        // Resgate total: absorve resíduos de cêntimos para não deixar saldo morto
        if (Math.Abs(creditBalance - creditToDebit) <= 0.01m)
            creditToDebit = creditBalance;

        var now = DateTime.UtcNow;
        var remainingToDebit = creditToDebit;

        foreach (var credit in credits)
        {
            if (remainingToDebit <= 0) break;

            var debitAmount = Math.Min(credit.CurrentBalance, remainingToDebit);
            if (debitAmount <= 0) continue;

            credit.CurrentBalance -= debitAmount;
            credit.UpdatedOn = now;
            credit.UpdatedBy = request.UserEmail;
            if (credit.CurrentBalance == 0)
                credit.Status = StoreCreditStatus.FullyUsed;

            salesDb.StoreCreditTransactions.Add(new StoreCreditTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                StoreCreditId = credit.Id,
                Amount = -debitAmount,
                BalanceAfter = credit.CurrentBalance,
                TransactionType = StoreCreditTransactionType.CashRedemption,
                TransactionDate = now,
                ProcessedBy = request.UserEmail,
                Notes = request.Notes ?? $"Resgate em dinheiro: {request.Amount:C} entregues (taxa {rate:0.##})",
                CreatedOn = now,
                CreatedBy = request.UserEmail
            });

            remainingToDebit -= debitAmount;
        }

        // Registo histórico do dinheiro entregue (apenas auditoria; o saldo vive no crédito em loja)
        var cashTransaction = new SupplierCashBalanceTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            Amount = -request.Amount,
            TransactionType = SupplierCashBalanceTransactionType.CashRedemption,
            TransactionDate = now,
            ProcessedBy = request.UserEmail,
            Notes = request.Notes ?? $"Resgate em dinheiro (crédito descontado: {creditToDebit:C})",
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.SupplierCashBalanceTransactions.Add(cashTransaction);
        await salesDb.SaveChangesAsync(cancellationToken);

        var newCreditBalance = creditBalance - creditToDebit;

        return new ProcessCashRedemptionResponse(
            cashTransaction.ExternalId, request.SupplierId, supplier.Name,
            request.Amount, creditToDebit, creditBalance, newCreditBalance,
            cashTransaction.TransactionDate, cashTransaction.ProcessedBy,
            $"Resgate de {request.Amount:C} processado (crédito descontado: {creditToDebit:C}). Crédito restante: {newCreditBalance:C}");
    }
}
