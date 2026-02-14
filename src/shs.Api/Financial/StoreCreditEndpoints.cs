using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Financial;

public static class StoreCreditEndpoints
{
    public static void MapStoreCreditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/store-credits").WithTags("Store Credits");

        group.MapGet("/supplier/{supplierId}", GetSupplierStoreCredits)
            .RequirePermission("reports.view");

        group.MapGet("/{externalId:guid}", GetStoreCreditById)
            .RequirePermission("reports.view");

        group.MapGet("/{externalId:guid}/transactions", GetStoreCreditTransactions)
            .RequirePermission("reports.view");

        group.MapPost("/", IssueStoreCredit)
            .RequirePermission("reports.view");

        group.MapPost("/{externalId:guid}/use", UseStoreCredit)
            .RequirePermission("pos.sales.create");

        group.MapPost("/use-by-supplier", UseStoreCreditBySupplier)
            .RequirePermission("pos.sales.create");

        group.MapPost("/{externalId:guid}/adjust", AdjustStoreCredit)
            .RequirePermission("reports.view");

        group.MapDelete("/{externalId:guid}", CancelStoreCredit)
            .RequirePermission("reports.view");
    }

    // Get all store credits for a supplier
    private static async Task<IResult> GetSupplierStoreCredits(
        ShsDbContext db,
        long supplierId,
        [FromQuery] StoreCreditStatus? status,
        CancellationToken ct)
    {
        var query = db.StoreCredits
            .Include(sc => sc.Supplier)
            .Where(sc => !sc.IsDeleted && sc.SupplierId == supplierId);

        if (status.HasValue)
        {
            query = query.Where(sc => sc.Status == status.Value);
        }

        var credits = await query
            .OrderByDescending(sc => sc.IssuedOn)
            .Select(sc => new
            {
                sc.ExternalId,
                sc.OriginalAmount,
                sc.CurrentBalance,
                sc.Status,
                sc.IssuedOn,
                sc.IssuedBy,
                sc.ExpiresOn,
                sc.Notes,
                SourceSettlement = sc.SourceSettlement != null ? new
                {
                    sc.SourceSettlement.ExternalId,
                    sc.SourceSettlement.PeriodStart,
                    sc.SourceSettlement.PeriodEnd
                } : null
            })
            .ToListAsync(ct);

        var totalBalance = credits
            .Where(c => c.Status == StoreCreditStatus.Active)
            .Sum(c => c.CurrentBalance);

        return Results.Ok(new
        {
            SupplierId = supplierId,
            TotalActiveBalance = totalBalance,
            Credits = credits
        });
    }

    // Get store credit details by ID
    private static async Task<IResult> GetStoreCreditById(
        ShsDbContext db,
        Guid externalId,
        CancellationToken ct)
    {
        var credit = await db.StoreCredits
            .Include(sc => sc.Supplier)
            .Include(sc => sc.SourceSettlement)
            .Include(sc => sc.Transactions)
            .Where(sc => !sc.IsDeleted && sc.ExternalId == externalId)
            .Select(sc => new
            {
                sc.ExternalId,
                sc.SupplierId,
                SupplierName = sc.Supplier.Name,
                sc.OriginalAmount,
                sc.CurrentBalance,
                sc.Status,
                sc.IssuedOn,
                sc.IssuedBy,
                sc.ExpiresOn,
                sc.Notes,
                SourceSettlement = sc.SourceSettlement != null ? new
                {
                    sc.SourceSettlement.ExternalId,
                    sc.SourceSettlement.TotalSalesAmount,
                    sc.SourceSettlement.PeriodStart,
                    sc.SourceSettlement.PeriodEnd
                } : null,
                TransactionCount = sc.Transactions.Count,
                LastTransaction = sc.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new
                    {
                        t.TransactionDate,
                        t.Amount,
                        t.TransactionType
                    })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (credit == null)
        {
            return Results.NotFound(new { error = "Store credit not found." });
        }

        return Results.Ok(credit);
    }

    // Get transaction history for a store credit
    private static async Task<IResult> GetStoreCreditTransactions(
        ShsDbContext db,
        Guid externalId,
        CancellationToken ct)
    {
        var credit = await db.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == externalId, ct);

        if (credit == null)
        {
            return Results.NotFound(new { error = "Store credit not found." });
        }

        var transactions = await db.StoreCreditTransactions
            .Include(t => t.Sale)
            .Where(t => t.StoreCreditId == credit.Id)
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new
            {
                t.ExternalId,
                t.Amount,
                t.BalanceAfter,
                t.TransactionType,
                t.TransactionDate,
                t.ProcessedBy,
                t.Notes,
                Sale = t.Sale != null ? new
                {
                    t.Sale.ExternalId,
                    t.Sale.SaleNumber,
                    t.Sale.TotalAmount
                } : null
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            StoreCreditId = externalId,
            CurrentBalance = credit.CurrentBalance,
            Transactions = transactions
        });
    }

    // Manually issue store credit (not from settlement)
    private static async Task<IResult> IssueStoreCredit(
        ShsDbContext db,
        HttpContext httpContext,
        [FromBody] IssueStoreCreditRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var supplier = await db.Suppliers.FindAsync([req.SupplierId], ct);
        if (supplier == null)
        {
            return Results.BadRequest(new { error = "Supplier not found." });
        }

        if (req.Amount <= 0)
        {
            return Results.BadRequest(new { error = "Amount must be greater than zero." });
        }

        var now = DateTime.UtcNow;

        var storeCredit = new StoreCreditEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = req.SupplierId,
            OriginalAmount = req.Amount,
            CurrentBalance = req.Amount,
            IssuedOn = now,
            IssuedBy = userEmail,
            ExpiresOn = req.ExpiresOn,
            Status = StoreCreditStatus.Active,
            Notes = req.Notes,
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.StoreCredits.Add(storeCredit);
        await db.SaveChangesAsync(ct);

        // Create initial transaction
        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = storeCredit.Id,
            Amount = storeCredit.OriginalAmount,
            BalanceAfter = storeCredit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Issue,
            TransactionDate = now,
            ProcessedBy = userEmail,
            Notes = req.Notes ?? "Manual store credit issuance",
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.StoreCreditTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/store-credits/{storeCredit.ExternalId}", new
        {
            storeCredit.ExternalId,
            storeCredit.SupplierId,
            SupplierName = supplier.Name,
            storeCredit.OriginalAmount,
            storeCredit.CurrentBalance,
            storeCredit.Status,
            storeCredit.IssuedOn,
            storeCredit.IssuedBy
        });
    }

    // Use store credit by supplier (for POS - deducts from supplier's credits, oldest first)
    private static async Task<IResult> UseStoreCreditBySupplier(
        ShsDbContext db,
        HttpContext httpContext,
        [FromBody] UseStoreCreditBySupplierRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var supplier = await db.Suppliers.FindAsync([req.SupplierId], ct);
        if (supplier == null)
            return Results.NotFound(new { error = "Supplier not found." });

        var credits = await db.StoreCredits
            .Where(sc => !sc.IsDeleted && sc.SupplierId == req.SupplierId && sc.Status == StoreCreditStatus.Active)
            .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
            .OrderBy(sc => sc.IssuedOn)
            .ToListAsync(ct);

        var totalAvailable = credits.Sum(c => c.CurrentBalance);
        if (req.Amount <= 0)
            return Results.BadRequest(new { error = "O valor deve ser positivo." });
        if (req.Amount > totalAvailable)
            return Results.BadRequest(new { error = $"Saldo insuficiente. Disponível: {totalAvailable:C}" });

        var remainingToUse = req.Amount;
        var usedCredits = new List<object>();
        var now = DateTime.UtcNow;

        foreach (var credit in credits)
        {
            if (remainingToUse <= 0) break;

            var useAmount = Math.Min(credit.CurrentBalance, remainingToUse);
            if (useAmount <= 0) continue;

            credit.CurrentBalance -= useAmount;
            credit.UpdatedOn = now;
            credit.UpdatedBy = userEmail;
            if (credit.CurrentBalance == 0)
                credit.Status = StoreCreditStatus.FullyUsed;

            var transaction = new StoreCreditTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                StoreCreditId = credit.Id,
                SaleId = req.SaleId,
                Amount = -useAmount,
                BalanceAfter = credit.CurrentBalance,
                TransactionType = StoreCreditTransactionType.Use,
                TransactionDate = now,
                ProcessedBy = userEmail,
                Notes = req.Notes ?? "Crédito usado em compra",
                CreatedOn = now,
                CreatedBy = userEmail
            };
            db.StoreCreditTransactions.Add(transaction);

            usedCredits.Add(new { credit.ExternalId, AmountUsed = useAmount, RemainingBalance = credit.CurrentBalance });
            remainingToUse -= useAmount;
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            SupplierId = req.SupplierId,
            SupplierName = supplier.Name,
            TotalUsed = req.Amount,
            CreditsUsed = usedCredits
        });
    }

    // Use store credit (deduct from balance)
    private static async Task<IResult> UseStoreCredit(
        ShsDbContext db,
        HttpContext httpContext,
        Guid externalId,
        [FromBody] UseStoreCreditRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var credit = await db.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == externalId, ct);

        if (credit == null)
        {
            return Results.NotFound(new { error = "Store credit not found." });
        }

        if (credit.Status != StoreCreditStatus.Active)
        {
            return Results.BadRequest(new { error = "Store credit is not active." });
        }

        if (credit.ExpiresOn.HasValue && credit.ExpiresOn.Value < DateTime.UtcNow)
        {
            return Results.BadRequest(new { error = "Store credit has expired." });
        }

        if (req.Amount <= 0)
        {
            return Results.BadRequest(new { error = "Amount must be greater than zero." });
        }

        if (req.Amount > credit.CurrentBalance)
        {
            return Results.BadRequest(new { error = $"Insufficient balance. Available: {credit.CurrentBalance:C}" });
        }

        var now = DateTime.UtcNow;

        // Deduct from balance
        credit.CurrentBalance -= req.Amount;
        credit.UpdatedOn = now;
        credit.UpdatedBy = userEmail;

        // Update status if fully used
        if (credit.CurrentBalance == 0)
        {
            credit.Status = StoreCreditStatus.FullyUsed;
        }

        // Create transaction
        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = credit.Id,
            SaleId = req.SaleId,
            Amount = -req.Amount, // Negative for deduction
            BalanceAfter = credit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Use,
            TransactionDate = now,
            ProcessedBy = userEmail,
            Notes = req.Notes ?? "Store credit used in purchase",
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.StoreCreditTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            credit.ExternalId,
            AmountUsed = req.Amount,
            RemainingBalance = credit.CurrentBalance,
            credit.Status
        });
    }

    // Manually adjust store credit balance
    private static async Task<IResult> AdjustStoreCredit(
        ShsDbContext db,
        HttpContext httpContext,
        Guid externalId,
        [FromBody] AdjustStoreCreditRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var credit = await db.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == externalId, ct);

        if (credit == null)
        {
            return Results.NotFound(new { error = "Store credit not found." });
        }

        if (credit.Status != StoreCreditStatus.Active)
        {
            return Results.BadRequest(new { error = "Store credit is not active." });
        }

        var now = DateTime.UtcNow;
        var oldBalance = credit.CurrentBalance;

        // Adjust balance
        credit.CurrentBalance += req.AdjustmentAmount;

        if (credit.CurrentBalance < 0)
        {
            return Results.BadRequest(new { error = "Adjustment would result in negative balance." });
        }

        credit.UpdatedOn = now;
        credit.UpdatedBy = userEmail;

        // Update status
        if (credit.CurrentBalance == 0)
        {
            credit.Status = StoreCreditStatus.FullyUsed;
        }

        // Create transaction
        var transaction = new StoreCreditTransactionEntity
        {
            ExternalId = Guid.NewGuid(),
            StoreCreditId = credit.Id,
            Amount = req.AdjustmentAmount,
            BalanceAfter = credit.CurrentBalance,
            TransactionType = StoreCreditTransactionType.Adjustment,
            TransactionDate = now,
            ProcessedBy = userEmail,
            Notes = req.Reason ?? "Manual adjustment",
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.StoreCreditTransactions.Add(transaction);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            credit.ExternalId,
            OldBalance = oldBalance,
            AdjustmentAmount = req.AdjustmentAmount,
            NewBalance = credit.CurrentBalance,
            credit.Status,
            Reason = req.Reason
        });
    }

    // Cancel/void a store credit
    private static async Task<IResult> CancelStoreCredit(
        ShsDbContext db,
        HttpContext httpContext,
        Guid externalId,
        [FromQuery] string? reason,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var credit = await db.StoreCredits
            .FirstOrDefaultAsync(sc => !sc.IsDeleted && sc.ExternalId == externalId, ct);

        if (credit == null)
        {
            return Results.NotFound(new { error = "Store credit not found." });
        }

        if (credit.Status == StoreCreditStatus.Cancelled)
        {
            return Results.BadRequest(new { error = "Store credit is already cancelled." });
        }

        var now = DateTime.UtcNow;
        var remainingBalance = credit.CurrentBalance;

        // Set balance to zero and mark as cancelled
        credit.CurrentBalance = 0;
        credit.Status = StoreCreditStatus.Cancelled;
        credit.UpdatedOn = now;
        credit.UpdatedBy = userEmail;

        // Create cancellation transaction if there was remaining balance
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
                ProcessedBy = userEmail,
                Notes = reason ?? "Store credit cancelled",
                CreatedOn = now,
                CreatedBy = userEmail
            };

            db.StoreCreditTransactions.Add(transaction);
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            credit.ExternalId,
            credit.Status,
            CancelledBalance = remainingBalance,
            Message = "Store credit cancelled successfully."
        });
    }
}

// DTOs
public record IssueStoreCreditRequest(
    long SupplierId,
    decimal Amount,
    DateTime? ExpiresOn,
    string? Notes
);

public record UseStoreCreditRequest(
    decimal Amount,
    long? SaleId,
    string? Notes
);

public record UseStoreCreditBySupplierRequest(
    long SupplierId,
    decimal Amount,
    long? SaleId,
    string? Notes
);

public record AdjustStoreCreditRequest(
    decimal AdjustmentAmount,
    string? Reason
);
