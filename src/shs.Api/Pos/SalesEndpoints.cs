using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Pos;

public static class SalesEndpoints
{
    public static void MapSalesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pos/sales").WithTags("POS - Sales");

        group.MapPost("/", ProcessSale).RequirePermission("pos.sales.create");
        group.MapGet("/today", GetSalesToday).RequirePermission("pos.sales.view");
        group.MapGet("/{externalId:guid}", GetSaleById).RequirePermission("pos.sales.view");
        group.MapGet("/", SearchSales).RequirePermission("pos.sales.view");
    }

    // ── Helpers ──

    private static string GetUserId(HttpContext ctx) =>
        ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? ctx.User.FindFirstValue("user_id")
        ?? ctx.User.FindFirstValue("sub")
        ?? "unknown";

    // ── Process Sale ──

    /// <summary>
    /// Process a sale: validate items, calculate totals, create sale record,
    /// update item statuses to Sold, and track commission.
    /// </summary>
    private static async Task<IResult> ProcessSale(
        [FromBody] ProcessSaleRequest req,
        HttpContext httpCtx,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var userId = GetUserId(httpCtx);

        // ── Validate request ──
        if (req.Items is null || req.Items.Length == 0)
            return Results.BadRequest(new { error = "A venda deve ter pelo menos um item." });

        if (req.Payments is null || req.Payments.Length == 0)
            return Results.BadRequest(new { error = "A venda deve ter pelo menos um pagamento." });

        if (req.Payments.Length > 2)
            return Results.BadRequest(new { error = "Máximo de 2 métodos de pagamento por venda." });

        if (req.DiscountPercentage.HasValue && (req.DiscountPercentage < 0 || req.DiscountPercentage > 100))
            return Results.BadRequest(new { error = "A percentagem de desconto deve estar entre 0 e 100." });

        // ── Validate cash register ──
        var register = await db.CashRegisters
            .FirstOrDefaultAsync(r => r.ExternalId == req.CashRegisterId
                                      && r.Status == CashRegisterStatus.Open, ct);

        if (register is null)
            return Results.BadRequest(new { error = "Caixa não encontrada ou não está aberta." });

        if (register.OperatorUserId != userId)
            return Results.Forbid();

        // ── Load and validate items ──
        var itemExternalIds = req.Items.Select(i => i.ItemExternalId).ToArray();

        var items = await db.Items
            .Where(i => itemExternalIds.Contains(i.ExternalId) && !i.IsDeleted)
            .Include(i => i.Brand)
            .Include(i => i.Supplier)
            .ToListAsync(ct);

        if (items.Count != req.Items.Length)
        {
            var foundIds = items.Select(i => i.ExternalId).ToHashSet();
            var missing = req.Items.Where(ri => !foundIds.Contains(ri.ItemExternalId)).ToList();
            return Results.BadRequest(new { error = $"Item(ns) não encontrado(s): {string.Join(", ", missing.Select(m => m.ItemExternalId))}" });
        }

        // Validate all items are ToSell
        var nonSellable = items.Where(i => i.Status != ItemStatus.ToSell).ToList();
        if (nonSellable.Count > 0)
        {
            var ids = string.Join(", ", nonSellable.Select(i => i.IdentificationNumber));
            return Results.BadRequest(new { error = $"Os seguintes itens não estão disponíveis para venda: {ids}" });
        }

        // ── Calculate prices ──
        var saleItems = new List<SaleItemEntity>();
        decimal subtotal = 0;

        foreach (var reqItem in req.Items)
        {
            var item = items.First(i => i.ExternalId == reqItem.ItemExternalId);
            var unitPrice = item.EvaluatedPrice;
            var itemDiscount = reqItem.DiscountAmount;

            if (itemDiscount < 0)
                return Results.BadRequest(new { error = $"O desconto do item {item.IdentificationNumber} não pode ser negativo." });

            if (itemDiscount > unitPrice)
                return Results.BadRequest(new { error = $"O desconto do item {item.IdentificationNumber} não pode ser maior que o preço." });

            var finalPrice = unitPrice - itemDiscount;

            saleItems.Add(new SaleItemEntity
            {
                ItemId = item.Id,
                UnitPrice = unitPrice,
                DiscountAmount = itemDiscount,
                FinalPrice = finalPrice,
            });

            subtotal += unitPrice;
        }

        // Calculate global discount
        var discountPercentage = req.DiscountPercentage ?? 0;
        var percentageDiscount = subtotal * discountPercentage / 100;
        var itemDiscountsTotal = saleItems.Sum(si => si.DiscountAmount);
        var totalDiscount = percentageDiscount + itemDiscountsTotal;
        var totalAmount = subtotal - totalDiscount;

        if (totalAmount < 0)
            return Results.BadRequest(new { error = "O valor total da venda não pode ser negativo." });

        // Apply percentage discount proportionally to item final prices
        if (discountPercentage > 0)
        {
            foreach (var si in saleItems)
            {
                var proportionalDiscount = si.FinalPrice * discountPercentage / 100;
                si.FinalPrice -= proportionalDiscount;
                si.DiscountAmount += proportionalDiscount;
            }
        }

        // ── Validate payments ──
        var paymentTotal = req.Payments.Sum(p => p.Amount);
        if (paymentTotal < totalAmount)
            return Results.BadRequest(new { error = $"O total dos pagamentos ({paymentTotal:F2}€) é inferior ao valor da venda ({totalAmount:F2}€)." });

        foreach (var p in req.Payments)
        {
            if (p.Amount <= 0)
                return Results.BadRequest(new { error = "O valor de cada pagamento deve ser positivo." });

            if (!Enum.TryParse<PaymentMethodType>(p.Method, out _))
                return Results.BadRequest(new { error = $"Método de pagamento inválido: {p.Method}" });

            if (p.Method == nameof(PaymentMethodType.StoreCredit) && !p.SupplierId.HasValue)
                return Results.BadRequest(new { error = "Ao usar Crédito em Loja, deve identificar o fornecedor (SupplierId)." });
        }

        // Validate store credit balances for StoreCredit payments
        foreach (var p in req.Payments.Where(x => x.Method == nameof(PaymentMethodType.StoreCredit)))
        {
            if (!p.SupplierId.HasValue) continue;

            var creditBalance = await db.StoreCredits
                .Where(sc => !sc.IsDeleted && sc.SupplierId == p.SupplierId && sc.Status == StoreCreditStatus.Active)
                .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
                .SumAsync(sc => sc.CurrentBalance, ct);

            if (creditBalance < p.Amount)
            {
                var supplier = await db.Suppliers.FindAsync([p.SupplierId.Value], ct);
                var name = supplier?.Name ?? p.SupplierId.ToString();
                return Results.BadRequest(new
                {
                    error = $"Crédito insuficiente para o fornecedor {name}. Disponível: {creditBalance:C}",
                    SupplierId = p.SupplierId,
                    AvailableBalance = creditBalance
                });
            }
        }

        // Log warning for high discounts
        if (discountPercentage > 10)
        {
            // In production, this would check role permissions
            // For now, just allow it
        }

        // ── Generate sale number: V{YYYYMMDD}-{DailySequence:000} ──
        var today = DateTime.UtcNow.Date;
        var todayPrefix = $"V{today:yyyyMMdd}-";

        var lastSaleNumber = await db.Sales
            .IgnoreQueryFilters()
            .Where(s => s.SaleNumber.StartsWith(todayPrefix))
            .OrderByDescending(s => s.SaleNumber)
            .Select(s => s.SaleNumber)
            .FirstOrDefaultAsync(ct);

        int nextSeq = 1;
        if (lastSaleNumber is not null)
        {
            var seqPart = lastSaleNumber[(todayPrefix.Length)..];
            if (int.TryParse(seqPart, out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        var saleNumber = $"{todayPrefix}{nextSeq:000}";

        // ── Create sale ──
        var sale = new SaleEntity
        {
            ExternalId = Guid.NewGuid(),
            SaleNumber = saleNumber,
            CashRegisterId = register.Id,
            SaleDate = DateTime.UtcNow,
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = totalDiscount,
            TotalAmount = totalAmount,
            DiscountReason = req.DiscountReason?.Trim(),
            Status = SaleStatus.Active,
            Notes = req.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId,
            Items = saleItems,
            Payments = req.Payments.Select(p => new SalePaymentEntity
            {
                PaymentMethod = Enum.Parse<PaymentMethodType>(p.Method),
                Amount = p.Amount,
                Reference = p.Reference?.Trim(),
                SupplierId = p.SupplierId,
            }).ToList()
        };

        db.Sales.Add(sale);

        // Persist Sale first so it gets a database-generated Id (required for Items.SaleId FK)
        await db.SaveChangesAsync(ct);

        // ── Deduct store credits for StoreCredit payments ──
        foreach (var payment in sale.Payments.Where(p => p.PaymentMethod == PaymentMethodType.StoreCredit && p.SupplierId.HasValue))
        {
            var credits = await db.StoreCredits
                .Where(sc => !sc.IsDeleted && sc.SupplierId == payment.SupplierId!.Value && sc.Status == StoreCreditStatus.Active)
                .Where(sc => sc.ExpiresOn == null || sc.ExpiresOn > DateTime.UtcNow)
                .OrderBy(sc => sc.IssuedOn)
                .ToListAsync(ct);

            var remainingToUse = payment.Amount;
            foreach (var credit in credits)
            {
                if (remainingToUse <= 0) break;
                var useAmount = Math.Min(credit.CurrentBalance, remainingToUse);
                if (useAmount <= 0) continue;

                credit.CurrentBalance -= useAmount;
                credit.UpdatedOn = DateTime.UtcNow;
                credit.UpdatedBy = userId;
                if (credit.CurrentBalance == 0)
                    credit.Status = StoreCreditStatus.FullyUsed;

                var transaction = new StoreCreditTransactionEntity
                {
                    ExternalId = Guid.NewGuid(),
                    StoreCreditId = credit.Id,
                    SaleId = sale.Id,
                    Amount = -useAmount,
                    BalanceAfter = credit.CurrentBalance,
                    TransactionType = StoreCreditTransactionType.Use,
                    TransactionDate = DateTime.UtcNow,
                    ProcessedBy = userId,
                    Notes = "Crédito usado em compra",
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = userId
                };
                db.StoreCreditTransactions.Add(transaction);
                payment.StoreCreditId = credit.Id; // Link to first credit used for audit
                remainingToUse -= useAmount;
            }
        }

        await db.SaveChangesAsync(ct);

        // ── Update item statuses to Sold ──
        foreach (var item in items)
        {
            item.Status = ItemStatus.Sold;
            item.SaleId = sale.Id;
            item.SoldAt = DateTime.UtcNow;
            item.FinalSalePrice = saleItems.First(si => si.ItemId == item.Id).FinalPrice;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = userId;

            // Calculate commission amounts (PorcInLoja + PorcInDinheiro) for settlement
            if (item.AcquisitionType == AcquisitionType.Consignment && item.SupplierId.HasValue && item.Supplier != null)
            {
                var finalSalePrice = item.FinalSalePrice.Value;
                var porcInLoja = item.Supplier.CreditPercentageInStore / 100m;
                var porcInDinheiro = item.Supplier.CashRedemptionPercentage / 100m;
                item.CommissionAmount = finalSalePrice * (porcInLoja + porcInDinheiro);
                item.CommissionPercentage = item.Supplier.CreditPercentageInStore + item.Supplier.CashRedemptionPercentage;
            }
        }

        await db.SaveChangesAsync(ct);

        // Calculate change (for cash payments)
        var change = paymentTotal > totalAmount ? paymentTotal - totalAmount : 0;

        return Results.Created(
            $"/api/pos/sales/{sale.ExternalId}",
            new ProcessSaleResponse(
                sale.ExternalId,
                sale.SaleNumber,
                sale.SaleDate,
                sale.Subtotal,
                sale.DiscountPercentage,
                sale.DiscountAmount,
                sale.TotalAmount,
                change,
                sale.Items.Count,
                register.OperatorName
            )
        );
    }

    // ── Get Sale By ID ──

    /// <summary>
    /// Get a sale with full details: items (name, brand, supplier), payments, cashier.
    /// </summary>
    private static async Task<IResult> GetSaleById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var sale = await db.Sales
            .Include(s => s.CashRegister)
            .Include(s => s.Items)
                .ThenInclude(si => si.Item)
                    .ThenInclude(i => i.Brand)
            .Include(s => s.Items)
                .ThenInclude(si => si.Item)
                    .ThenInclude(i => i.Supplier)
            .Include(s => s.Payments)
            .FirstOrDefaultAsync(s => s.ExternalId == externalId, ct);

        if (sale is null)
            return Results.NotFound(new { error = "Venda não encontrada." });

        return Results.Ok(new SaleDetailResponse(
            sale.ExternalId,
            sale.SaleNumber,
            sale.SaleDate,
            sale.Subtotal,
            sale.DiscountPercentage,
            sale.DiscountAmount,
            sale.TotalAmount,
            sale.DiscountReason,
            sale.Status.ToString(),
            sale.Notes,
            sale.CashRegister.OperatorName,
            sale.CashRegister.RegisterNumber,
            sale.Items.Select(si => new SaleItemDetail(
                si.Item.ExternalId,
                si.Item.IdentificationNumber,
                si.Item.Name,
                si.Item.Brand.Name,
                si.Item.Size,
                si.Item.Color,
                si.Item.Supplier?.Name,
                si.UnitPrice,
                si.DiscountAmount,
                si.FinalPrice
            )).ToList(),
            sale.Payments.Select(p => new SalePaymentDetail(
                p.PaymentMethod.ToString(),
                p.Amount,
                p.Reference
            )).ToList(),
            sale.CreatedOn
        ));
    }

    // ── Get Today's Sales ──

    /// <summary>
    /// Summary of today's sales: count, revenue, average ticket, by payment method.
    /// </summary>
    private static async Task<IResult> GetSalesToday(
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todaySales = await db.Sales
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .Include(s => s.Payments)
            .Include(s => s.Items)
            .ToListAsync(ct);

        var salesCount = todaySales.Count;
        var totalRevenue = todaySales.Sum(s => s.TotalAmount);
        var averageTicket = salesCount > 0 ? totalRevenue / salesCount : 0;
        var totalItems = todaySales.Sum(s => s.Items.Count);

        var byPaymentMethod = todaySales
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => new PaymentMethodSummary(g.Count(), g.Sum(p => p.Amount))
            );

        var recentSales = todaySales
            .OrderByDescending(s => s.SaleDate)
            .Take(10)
            .Select(s => new SaleListItem(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString(),
                string.Join(", ", s.Payments.Select(p => GetPaymentLabel(p.PaymentMethod)))
            ))
            .ToList();

        return Results.Ok(new TodaySalesResponse(
            salesCount,
            totalRevenue,
            averageTicket,
            totalItems,
            byPaymentMethod,
            recentSales
        ));
    }

    // ── Search Sales ──

    /// <summary>
    /// Search and paginate sales with optional date filters.
    /// </summary>
    private static async Task<IResult> SearchSales(
        [FromServices] ShsDbContext db,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Sales
            .Include(s => s.CashRegister)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .AsQueryable();

        if (dateFrom.HasValue)
            query = query.Where(s => s.SaleDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(s => s.SaleDate < dateTo.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(s => s.SaleNumber.ToLower().Contains(term)
                                     || s.CashRegister.OperatorName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(ct);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SaleListItem(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString(),
                string.Join(", ", s.Payments.Select(p => GetPaymentLabel(p.PaymentMethod)))
            ))
            .ToListAsync(ct);

        return Results.Ok(new SalesPagedResult(
            sales,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        ));
    }

    private static string GetPaymentLabel(PaymentMethodType method) => method switch
    {
        PaymentMethodType.Cash => "Dinheiro",
        PaymentMethodType.CreditCard => "Cartão Crédito",
        PaymentMethodType.DebitCard => "Cartão Débito",
        PaymentMethodType.PIX => "PIX",
        PaymentMethodType.StoreCredit => "Crédito Loja",
        _ => method.ToString()
    };
}

// ── Request DTOs ──

public record ProcessSaleRequest(
    Guid CashRegisterId,
    SaleItemRequest[] Items,
    SalePaymentRequest[] Payments,
    decimal? DiscountPercentage,
    string? DiscountReason,
    Guid? CustomerExternalId,
    string? Notes
);

public record SaleItemRequest(
    Guid ItemExternalId,
    decimal DiscountAmount
);

public record SalePaymentRequest(
    string Method,
    decimal Amount,
    string? Reference,
    /// <summary>Required when Method is StoreCredit - identifies which supplier's credit to use</summary>
    long? SupplierId
);

// ── Response DTOs ──

public record ProcessSaleResponse(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    decimal Change,
    int ItemCount,
    string CashierName
);

public record SaleDetailResponse(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? DiscountReason,
    string Status,
    string? Notes,
    string CashierName,
    int RegisterNumber,
    List<SaleItemDetail> Items,
    List<SalePaymentDetail> Payments,
    DateTime CreatedOn
);

public record SaleItemDetail(
    Guid ItemExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string? SupplierName,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal FinalPrice
);

public record SalePaymentDetail(
    string Method,
    decimal Amount,
    string? Reference
);

public record TodaySalesResponse(
    int SalesCount,
    decimal TotalRevenue,
    decimal AverageTicket,
    int TotalItems,
    Dictionary<string, PaymentMethodSummary> ByPaymentMethod,
    List<SaleListItem> RecentSales
);

public record PaymentMethodSummary(int Count, decimal Total);

public record SaleListItem(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    int ItemCount,
    string Status,
    string PaymentMethods
);

public record SalesPagedResult(
    List<SaleListItem> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
