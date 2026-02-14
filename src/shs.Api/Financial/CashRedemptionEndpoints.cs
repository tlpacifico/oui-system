using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Financial;

public static class CashRedemptionEndpoints
{
    public static void MapCashRedemptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cash-redemptions").WithTags("Cash Redemptions");

        group.MapGet("/supplier/{supplierId}/balance", GetSupplierCashBalance)
            .RequirePermission("reports.view");

        group.MapGet("/supplier/{supplierId}/history", GetSupplierCashRedemptionHistory)
            .RequirePermission("reports.view");

        group.MapPost("/", ProcessCashRedemption)
            .RequirePermission("reports.view");
    }

    /// <summary>
    /// Get supplier's available cash redemption balance (PorcInDinheiro)
    /// </summary>
    private static async Task<IResult> GetSupplierCashBalance(
        ShsDbContext db,
        long supplierId,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers.FindAsync([supplierId], ct);
        if (supplier == null)
            return Results.NotFound(new { error = "Supplier not found." });

        var balance = await db.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == supplierId)
            .SumAsync(t => t.Amount, ct);

        return Results.Ok(new
        {
            SupplierId = supplierId,
            SupplierName = supplier.Name,
            AvailableBalance = balance,
            CreditPercentageInStore = supplier.CreditPercentageInStore,
            CashRedemptionPercentage = supplier.CashRedemptionPercentage
        });
    }

    /// <summary>
    /// Get supplier's cash balance transaction history
    /// </summary>
    private static async Task<IResult> GetSupplierCashRedemptionHistory(
        ShsDbContext db,
        long supplierId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var supplier = await db.Suppliers.FindAsync([supplierId], ct);
        if (supplier == null)
            return Results.NotFound(new { error = "Supplier not found." });

        var query = db.SupplierCashBalanceTransactions
            .Include(t => t.Settlement)
            .Where(t => t.SupplierId == supplierId)
            .OrderByDescending(t => t.TransactionDate);

        var total = await query.CountAsync(ct);

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.ExternalId,
                t.Amount,
                t.TransactionType,
                t.TransactionDate,
                t.ProcessedBy,
                t.Notes,
                SettlementPeriod = t.Settlement != null
                    ? $"{t.Settlement.PeriodStart:yyyy-MM-dd} - {t.Settlement.PeriodEnd:yyyy-MM-dd}"
                    : null
            })
            .ToListAsync(ct);

        var balance = await db.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == supplierId)
            .SumAsync(t => t.Amount, ct);

        return Results.Ok(new
        {
            SupplierId = supplierId,
            SupplierName = supplier.Name,
            CurrentBalance = balance,
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Transactions = transactions
        });
    }

    /// <summary>
    /// Process cash redemption - supplier withdraws their PorcInDinheiro balance
    /// </summary>
    private static async Task<IResult> ProcessCashRedemption(
        ShsDbContext db,
        HttpContext httpContext,
        [FromBody] ProcessCashRedemptionRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var supplier = await db.Suppliers.FindAsync([req.SupplierId], ct);
        if (supplier == null)
            return Results.NotFound(new { error = "Supplier not found." });

        var currentBalance = await db.SupplierCashBalanceTransactions
            .Where(t => t.SupplierId == req.SupplierId)
            .SumAsync(t => t.Amount, ct);

        if (req.Amount <= 0)
            return Results.BadRequest(new { error = "O valor deve ser positivo." });

        if (req.Amount > currentBalance)
            return Results.BadRequest(new
            {
                error = $"Saldo insuficiente. Disponível: {currentBalance:C}",
                AvailableBalance = currentBalance
            });

        var now = DateTime.UtcNow;

        var transaction = new SupplierCashBalanceTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = req.SupplierId,
            Amount = -req.Amount,
            TransactionType = SupplierCashBalanceTransactionType.CashRedemption,
            TransactionDate = now,
            ProcessedBy = userEmail,
            Notes = req.Notes ?? "Resgate em dinheiro",
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.SupplierCashBalanceTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        var newBalance = currentBalance - req.Amount;

        return Results.Ok(new
        {
            transaction.ExternalId,
            SupplierId = req.SupplierId,
            SupplierName = supplier.Name,
            AmountRedeemed = req.Amount,
            PreviousBalance = currentBalance,
            NewBalance = newBalance,
            transaction.TransactionDate,
            transaction.ProcessedBy,
            Message = $"Resgate de {req.Amount:C} processado. Novo saldo: {newBalance:C}"
        });
    }
}

public record ProcessCashRedemptionRequest(
    long SupplierId,
    decimal Amount,
    string? Notes
);
