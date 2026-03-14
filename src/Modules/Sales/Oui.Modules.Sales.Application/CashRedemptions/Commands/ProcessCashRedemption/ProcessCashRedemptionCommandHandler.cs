using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
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

        var currentBalance = await salesDb.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == request.SupplierId)
            .SumAsync(t => t.Amount, cancellationToken);

        if (request.Amount <= 0)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.AmountMustBePositive);

        if (request.Amount > currentBalance)
            return Result.Failure<ProcessCashRedemptionResponse>(CashRedemptionErrors.InsufficientBalance(currentBalance));

        var now = DateTime.UtcNow;

        var transaction = new SupplierCashBalanceTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = request.SupplierId,
            Amount = -request.Amount,
            TransactionType = SupplierCashBalanceTransactionType.CashRedemption,
            TransactionDate = now,
            ProcessedBy = request.UserEmail,
            Notes = request.Notes ?? "Resgate em dinheiro",
            CreatedOn = now,
            CreatedBy = request.UserEmail
        };

        salesDb.SupplierCashBalanceTransactions.Add(transaction);
        await salesDb.SaveChangesAsync(cancellationToken);

        var newBalance = currentBalance - request.Amount;

        return new ProcessCashRedemptionResponse(
            transaction.ExternalId, request.SupplierId, supplier.Name,
            request.Amount, currentBalance, newBalance,
            transaction.TransactionDate, transaction.ProcessedBy,
            $"Resgate de {request.Amount:C} processado. Novo saldo: {newBalance:C}");
    }
}
