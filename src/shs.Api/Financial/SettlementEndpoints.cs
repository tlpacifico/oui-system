using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Financial;

public static class SettlementEndpoints
{
    public static void MapSettlementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settlements").WithTags("Settlements");

        group.MapGet("/pending-items", GetPendingSettlementItems)
            .RequirePermission("reports.view");

        group.MapPost("/calculate", CalculateSettlement)
            .RequirePermission("reports.view");

        group.MapPost("/", CreateSettlement)
            .RequirePermission("reports.view");

        group.MapGet("/", GetSettlements)
            .RequirePermission("reports.view");

        group.MapGet("/{externalId:guid}", GetSettlementById)
            .RequirePermission("reports.view");

        group.MapPost("/{externalId:guid}/process-payment", ProcessPayment)
            .RequirePermission("reports.view");

        group.MapDelete("/{externalId:guid}", CancelSettlement)
            .RequirePermission("reports.view");
    }

    // Get items pending settlement grouped by supplier
    private static async Task<IResult> GetPendingSettlementItems(
        ShsDbContext db,
        [FromQuery] long? supplierId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken ct)
    {
        var query = db.Items
            .Include(i => i.Supplier)
            .Include(i => i.Brand)
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId != null);

        // Filter by supplier if provided
        if (supplierId.HasValue)
        {
            query = query.Where(i => i.SupplierId == supplierId.Value);
        }

        // Filter by date range if provided (convert to UTC for PostgreSQL)
        if (startDate.HasValue)
        {
            query = query.Where(i => i.UpdatedOn >= ToUtc(startDate.Value));
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.UpdatedOn < ToUtc(endDate.Value).AddDays(1));
        }

        // Get items with their sale information
        var items = await query
            .Select(i => new
            {
                ItemId = i.Id,
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                BrandName = i.Brand!.Name,
                i.EvaluatedPrice,
                i.FinalSalePrice,
                i.CommissionPercentage,
                i.CommissionAmount,
                i.SupplierId,
                SupplierName = i.Supplier!.Name,
                SupplierInitial = i.Supplier.Initial,
                i.UpdatedOn, // Sale date (when item was updated to Sold)
                IsSettled = db.SaleItems.Any(si => si.ItemId == i.Id && si.SettlementId != null)
            })
            .Where(i => !i.IsSettled) // Only unsettled items
            .ToListAsync(ct);

        // Group by supplier
        var groupedBySupplier = items
            .GroupBy(i => new { i.SupplierId, i.SupplierName, i.SupplierInitial })
            .Select(g => new
            {
                g.Key.SupplierId,
                g.Key.SupplierName,
                g.Key.SupplierInitial,
                ItemCount = g.Count(),
                TotalSalesAmount = g.Sum(i => i.FinalSalePrice ?? 0),
                Items = g.OrderByDescending(i => i.UpdatedOn).ToList()
            })
            .OrderBy(g => g.SupplierName)
            .ToList();

        return Results.Ok(groupedBySupplier);
    }

    // Calculate settlement amounts before creating (uses supplier's PorcInLoja and PorcInDinheiro)
    private static async Task<IResult> CalculateSettlement(
        ShsDbContext db,
        [FromBody] CalculateSettlementRequest req,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers.FindAsync([req.SupplierId], ct);
        if (supplier == null)
            return Results.BadRequest(new { error = "Supplier not found." });

        var periodStart = ToUtc(req.PeriodStart);
        var periodEnd = ToUtc(req.PeriodEnd).AddDays(1);

        var items = await db.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId == req.SupplierId &&
                       i.UpdatedOn >= periodStart &&
                       i.UpdatedOn < periodEnd)
            .Select(i => new
            {
                i.Id,
                i.FinalSalePrice,
                IsSettled = db.SaleItems.Any(si => si.ItemId == i.Id && si.SettlementId != null)
            })
            .Where(i => !i.IsSettled)
            .ToListAsync(ct);

        if (items.Count == 0)
            return Results.BadRequest(new { error = "No items found for settlement in this period." });

        var totalSalesAmount = items.Sum(i => i.FinalSalePrice ?? 0);
        var porcInLoja = supplier.CreditPercentageInStore / 100m;
        var porcInDinheiro = supplier.CashRedemptionPercentage / 100m;

        var storeCreditAmount = totalSalesAmount * porcInLoja;
        var cashRedemptionAmount = totalSalesAmount * porcInDinheiro;
        var netAmountToSupplier = storeCreditAmount + cashRedemptionAmount;
        var storeCommissionAmount = totalSalesAmount - netAmountToSupplier;

        return Results.Ok(new
        {
            SupplierId = req.SupplierId,
            SupplierName = supplier.Name,
            PeriodStart = req.PeriodStart,
            PeriodEnd = req.PeriodEnd,
            ItemCount = items.Count,
            TotalSalesAmount = totalSalesAmount,
            CreditPercentageInStore = supplier.CreditPercentageInStore,
            CashRedemptionPercentage = supplier.CashRedemptionPercentage,
            StoreCreditAmount = storeCreditAmount,
            CashRedemptionAmount = cashRedemptionAmount,
            NetAmountToSupplier = netAmountToSupplier,
            StoreCommissionAmount = storeCommissionAmount
        });
    }

    // Create a settlement
    private static async Task<IResult> CreateSettlement(
        ShsDbContext db,
        HttpContext httpContext,
        [FromBody] CreateSettlementRequest req,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        // Validate supplier exists
        var supplier = await db.Suppliers.FindAsync([req.SupplierId], ct);
        if (supplier == null)
        {
            return Results.BadRequest(new { error = "Supplier not found." });
        }

        var periodStart = ToUtc(req.PeriodStart);
        var periodEnd = ToUtc(req.PeriodEnd).AddDays(1);

        // Get items to settle (sold, consignment, not yet settled)
        var items = await db.Items
            .Where(i => !i.IsDeleted &&
                       i.Status == ItemStatus.Sold &&
                       i.AcquisitionType == AcquisitionType.Consignment &&
                       i.SupplierId == req.SupplierId &&
                       i.UpdatedOn >= periodStart &&
                       i.UpdatedOn < periodEnd)
            .Select(i => new { i.Id, i.FinalSalePrice })
            .ToListAsync(ct);

        // Filter out already settled items
        var itemIds = items.Select(i => i.Id).ToList();
        var alreadySettledIds = await db.SaleItems
            .Where(si => itemIds.Contains(si.ItemId) && si.SettlementId != null)
            .Select(si => si.ItemId)
            .ToListAsync(ct);

        items = items.Where(i => !alreadySettledIds.Contains(i.Id)).ToList();

        if (items.Count == 0)
        {
            return Results.BadRequest(new { error = "No unsettled items found for this supplier in the specified period." });
        }

        var totalSalesAmount = items.Sum(i => i.FinalSalePrice ?? 0);
        var porcInLoja = supplier.CreditPercentageInStore / 100m;
        var porcInDinheiro = supplier.CashRedemptionPercentage / 100m;
        var storeCreditAmount = totalSalesAmount * porcInLoja;
        var cashRedemptionAmount = totalSalesAmount * porcInDinheiro;
        var netAmountToSupplier = storeCreditAmount + cashRedemptionAmount;
        var storeCommissionAmount = totalSalesAmount - netAmountToSupplier;

        var now = DateTime.UtcNow;
        var settlement = new SettlementEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = req.SupplierId,
            PeriodStart = periodStart,
            PeriodEnd = ToUtc(req.PeriodEnd), // Use date-only end (no AddDays for storage)
            TotalSalesAmount = totalSalesAmount,
            CreditPercentageInStore = supplier.CreditPercentageInStore,
            CashRedemptionPercentage = supplier.CashRedemptionPercentage,
            StoreCreditAmount = storeCreditAmount,
            CashRedemptionAmount = cashRedemptionAmount,
            StoreCommissionAmount = storeCommissionAmount,
            NetAmountToSupplier = netAmountToSupplier,
            PaymentMethod = SettlementPaymentMethod.StoreCredit, // Both credits are issued
            Status = SettlementStatus.Pending,
            Notes = req.Notes,
            CreatedOn = now,
            CreatedBy = userEmail
        };

        db.Settlements.Add(settlement);
        await db.SaveChangesAsync(ct);

        // Link sale items to this settlement
        var saleItems = await db.SaleItems
            .Where(si => itemIds.Contains(si.ItemId))
            .ToListAsync(ct);

        foreach (var saleItem in saleItems)
        {
            saleItem.SettlementId = settlement.Id;
        }

        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/settlements/{settlement.ExternalId}", new
        {
            settlement.ExternalId,
            settlement.SupplierId,
            SupplierName = supplier.Name,
            settlement.PeriodStart,
            settlement.PeriodEnd,
            settlement.TotalSalesAmount,
            settlement.CreditPercentageInStore,
            settlement.CashRedemptionPercentage,
            settlement.StoreCreditAmount,
            settlement.CashRedemptionAmount,
            settlement.StoreCommissionAmount,
            settlement.NetAmountToSupplier,
            settlement.Status,
            ItemCount = items.Count,
            settlement.Notes,
            settlement.CreatedOn,
            settlement.CreatedBy
        });
    }

    // Get all settlements with filters
    private static async Task<IResult> GetSettlements(
        ShsDbContext db,
        [FromQuery] long? supplierId,
        [FromQuery] SettlementStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Settlements
            .Include(s => s.Supplier)
            .Where(s => !s.IsDeleted);

        if (supplierId.HasValue)
        {
            query = query.Where(s => s.SupplierId == supplierId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var total = await query.CountAsync(ct);

        var settlements = await query
            .OrderByDescending(s => s.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.ExternalId,
                s.SupplierId,
                SupplierName = s.Supplier.Name,
                SupplierInitial = s.Supplier.Initial,
                s.PeriodStart,
                s.PeriodEnd,
                s.TotalSalesAmount,
                s.CreditPercentageInStore,
                s.CashRedemptionPercentage,
                s.StoreCreditAmount,
                s.CashRedemptionAmount,
                s.StoreCommissionAmount,
                s.NetAmountToSupplier,
                s.Status,
                ItemCount = s.SaleItems.Count,
                s.PaidOn,
                s.PaidBy,
                s.CreatedOn,
                s.CreatedBy
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Data = settlements
        });
    }

    // Get settlement by ID with details
    private static async Task<IResult> GetSettlementById(
        ShsDbContext db,
        Guid externalId,
        CancellationToken ct)
    {
        var settlement = await db.Settlements
            .Include(s => s.Supplier)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Item)
                    .ThenInclude(i => i.Brand)
            .Include(s => s.StoreCredit)
            .Where(s => !s.IsDeleted && s.ExternalId == externalId)
            .Select(s => new
            {
                s.ExternalId,
                s.SupplierId,
                SupplierName = s.Supplier.Name,
                SupplierEmail = s.Supplier.Email,
                SupplierPhone = s.Supplier.PhoneNumber,
                s.PeriodStart,
                s.PeriodEnd,
                s.TotalSalesAmount,
                s.CreditPercentageInStore,
                s.CashRedemptionPercentage,
                s.StoreCreditAmount,
                s.CashRedemptionAmount,
                s.StoreCommissionAmount,
                s.NetAmountToSupplier,
                s.Status,
                s.PaidOn,
                s.PaidBy,
                s.Notes,
                s.CreatedOn,
                s.CreatedBy,
                StoreCredit = s.StoreCredit != null ? new
                {
                    s.StoreCredit.ExternalId,
                    s.StoreCredit.OriginalAmount,
                    s.StoreCredit.CurrentBalance,
                    s.StoreCredit.Status,
                    s.StoreCredit.IssuedOn
                } : null,
                Items = s.SaleItems.Select(si => new
                {
                    si.Item.ExternalId,
                    si.Item.IdentificationNumber,
                    si.Item.Name,
                    BrandName = si.Item.Brand!.Name,
                    si.Item.EvaluatedPrice,
                    si.FinalPrice,
                    SaleDate = si.Item.UpdatedOn
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (settlement == null)
        {
            return Results.NotFound(new { error = "Settlement not found." });
        }

        return Results.Ok(settlement);
    }

    // Process payment for a settlement
    private static async Task<IResult> ProcessPayment(
        ShsDbContext db,
        HttpContext httpContext,
        Guid externalId,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var settlement = await db.Settlements
            .Include(s => s.Supplier)
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.ExternalId == externalId, ct);

        if (settlement == null)
        {
            return Results.NotFound(new { error = "Settlement not found." });
        }

        if (settlement.Status == SettlementStatus.Paid)
        {
            return Results.BadRequest(new { error = "Settlement is already paid." });
        }

        if (settlement.Status == SettlementStatus.Cancelled)
        {
            return Results.BadRequest(new { error = "Settlement is cancelled." });
        }

        var now = DateTime.UtcNow;

        // Create store credit (PorcInLoja) if amount > 0
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
                IssuedBy = userEmail,
                Status = StoreCreditStatus.Active,
                CreatedOn = now,
                CreatedBy = userEmail
            };

            db.StoreCredits.Add(storeCredit);
            await db.SaveChangesAsync(ct);

            var transaction = new StoreCreditTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                StoreCreditId = storeCredit.Id,
                Amount = storeCredit.OriginalAmount,
                BalanceAfter = storeCredit.CurrentBalance,
                TransactionType = StoreCreditTransactionType.Issue,
                TransactionDate = now,
                ProcessedBy = userEmail,
                Notes = $"Issued from settlement {settlement.ExternalId}",
                CreatedOn = now,
                CreatedBy = userEmail
            };

            db.StoreCreditTransactions.Add(transaction);
            settlement.StoreCreditId = storeCredit.Id;
        }

        // Create cash redemption balance (PorcInDinheiro) if amount > 0
        if (settlement.CashRedemptionAmount > 0)
        {
            var cashTransaction = new SupplierCashBalanceTransactionEntity
            {
                ExternalId = Guid.NewGuid(),
                SupplierId = settlement.SupplierId,
                SettlementId = settlement.Id,
                Amount = settlement.CashRedemptionAmount,
                TransactionType = SupplierCashBalanceTransactionType.SettlementCredit,
                TransactionDate = now,
                ProcessedBy = userEmail,
                Notes = $"Credit from settlement {settlement.ExternalId}",
                CreatedOn = now,
                CreatedBy = userEmail
            };

            db.SupplierCashBalanceTransactions.Add(cashTransaction);
        }

        // Update settlement status
        settlement.Status = SettlementStatus.Paid;
        settlement.PaidOn = now;
        settlement.PaidBy = userEmail;
        settlement.UpdatedOn = now;
        settlement.UpdatedBy = userEmail;

        // Update all items in settlement to Paid status
        var itemIds = await db.SaleItems
            .Where(si => si.SettlementId == settlement.Id)
            .Select(si => si.ItemId)
            .ToListAsync(ct);

        var items = await db.Items
            .Where(i => itemIds.Contains(i.Id))
            .ToListAsync(ct);

        foreach (var item in items)
        {
            item.Status = ItemStatus.Paid;
            item.UpdatedOn = now;
            item.UpdatedBy = userEmail;
        }

        await db.SaveChangesAsync(ct);

        var messages = new List<string>();
        if (settlement.StoreCreditAmount > 0)
            messages.Add($"Crédito em loja: {settlement.StoreCreditAmount:C}");
        if (settlement.CashRedemptionAmount > 0)
            messages.Add($"Saldo para resgate em dinheiro: {settlement.CashRedemptionAmount:C}");

        return Results.Ok(new
        {
            settlement.ExternalId,
            settlement.Status,
            settlement.PaidOn,
            settlement.PaidBy,
            Message = string.Join(". ", messages)
        });
    }

    // Cancel a settlement
    private static async Task<IResult> CancelSettlement(
        ShsDbContext db,
        HttpContext httpContext,
        Guid externalId,
        CancellationToken ct)
    {
        var userEmail = httpContext.User.GetUserEmail();

        var settlement = await db.Settlements
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.ExternalId == externalId, ct);

        if (settlement == null)
        {
            return Results.NotFound(new { error = "Settlement not found." });
        }

        if (settlement.Status == SettlementStatus.Paid)
        {
            return Results.BadRequest(new { error = "Cannot cancel a paid settlement." });
        }

        var now = DateTime.UtcNow;

        // Unlink sale items from this settlement
        var saleItems = await db.SaleItems
            .Where(si => si.SettlementId == settlement.Id)
            .ToListAsync(ct);

        foreach (var saleItem in saleItems)
        {
            saleItem.SettlementId = null;
        }

        // Mark settlement as cancelled
        settlement.Status = SettlementStatus.Cancelled;
        settlement.UpdatedOn = now;
        settlement.UpdatedBy = userEmail;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new
        {
            settlement.ExternalId,
            settlement.Status,
            Message = "Settlement cancelled successfully."
        });
    }
    /// <summary>
    /// Converts DateTime to UTC for PostgreSQL compatibility. Npgsql requires UTC for timestamptz.
    /// </summary>
    private static DateTime ToUtc(DateTime dt) =>
        dt.Kind == DateTimeKind.Utc ? dt : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
}

// DTOs
public record CalculateSettlementRequest(
    long SupplierId,
    DateTime PeriodStart,
    DateTime PeriodEnd
);

public record CreateSettlementRequest(
    long SupplierId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string? Notes
);
