using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Pos;

public static class PosEndpoints
{
    public static void MapPosEndpoints(this IEndpointRouteBuilder app)
    {
        var registerGroup = app.MapGroup("/api/pos/register").WithTags("POS - Cash Register");

        registerGroup.MapPost("/open", OpenRegister).RequirePermission("pos.register.open");
        registerGroup.MapPost("/close", CloseRegister).RequirePermission("pos.register.close");
        registerGroup.MapGet("/current", GetCurrentRegister).RequirePermission("pos.register.view");
        registerGroup.MapGet("/status", GetAllRegistersStatus).RequirePermission("pos.register.view");
        registerGroup.MapGet("/{externalId:guid}", GetRegisterById).RequirePermission("pos.register.view");

        var posGroup = app.MapGroup("/api/pos").WithTags("POS");
        posGroup.MapGet("/suppliers", GetPosSuppliers).RequirePermission("pos.sales.create");
    }

    /// <summary>
    /// Get suppliers list for POS (e.g. when selecting supplier for Crédito em Loja payment).
    /// Returns minimal data: id, name, initial.
    /// </summary>
    private static async Task<IResult> GetPosSuppliers(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Suppliers.Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(sup =>
                sup.Name.ToLower().Contains(s) ||
                sup.Initial.ToLower().Contains(s));
        }

        var suppliers = await query
            .OrderBy(sup => sup.Name)
            .Select(sup => new { sup.Id, sup.Name, sup.Initial })
            .Take(200)
            .ToListAsync(ct);

        return Results.Ok(suppliers);
    }

    // ── Helpers ──

    private static string GetUserId(HttpContext ctx) =>
        ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? ctx.User.FindFirstValue("user_id")
        ?? ctx.User.FindFirstValue("sub")
        ?? "unknown";

    private static string GetUserName(HttpContext ctx) =>
        ctx.User.FindFirstValue("name")
        ?? ctx.User.FindFirstValue("display_name")
        ?? ctx.User.FindFirstValue(ClaimTypes.Name)
        ?? ctx.User.FindFirstValue(ClaimTypes.Email)
        ?? "Operador";

    // ── Open Register ──

    /// <summary>
    /// Open a new cash register for the current user.
    /// Only one register can be open per operator at a time.
    /// </summary>
    private static async Task<IResult> OpenRegister(
        [FromBody] OpenRegisterRequest req,
        HttpContext httpCtx,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (req.OpeningAmount < 0)
            return Results.BadRequest(new { error = "O valor de abertura não pode ser negativo." });

        var userId = GetUserId(httpCtx);

        // Check if this user already has an open register
        var existingOpen = await db.CashRegisters
            .AnyAsync(r => r.OperatorUserId == userId && r.Status == CashRegisterStatus.Open, ct);

        if (existingOpen)
            return Results.Conflict(new { error = "Já tem uma caixa aberta. Feche a caixa atual antes de abrir outra." });

        // Determine the next register number (simple auto-increment)
        var lastNumber = await db.CashRegisters
            .IgnoreQueryFilters()
            .MaxAsync(r => (int?)r.RegisterNumber, ct) ?? 0;

        var register = new CashRegisterEntity
        {
            ExternalId = Guid.NewGuid(),
            OperatorUserId = userId,
            OperatorName = GetUserName(httpCtx),
            RegisterNumber = lastNumber + 1,
            OpenedAt = DateTime.UtcNow,
            OpeningAmount = req.OpeningAmount,
            Status = CashRegisterStatus.Open,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = userId
        };

        db.CashRegisters.Add(register);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/pos/register/{register.ExternalId}",
            new RegisterResponse(
                register.ExternalId,
                register.RegisterNumber,
                register.OperatorName,
                register.OpenedAt,
                null, // closedAt
                register.OpeningAmount,
                null, // closingAmount
                null, // expectedAmount
                null, // discrepancy
                register.Status.ToString(),
                0,    // salesCount
                0m    // salesTotal
            )
        );
    }

    // ── Close Register ──

    /// <summary>
    /// Close the current user's open cash register.
    /// Calculates expected amount and discrepancy.
    /// </summary>
    private static async Task<IResult> CloseRegister(
        [FromBody] CloseRegisterRequest req,
        HttpContext httpCtx,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (req.ClosingAmount < 0)
            return Results.BadRequest(new { error = "O valor de fecho não pode ser negativo." });

        var userId = GetUserId(httpCtx);

        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Payments)
            .FirstOrDefaultAsync(r => r.ExternalId == req.RegisterExternalId
                                      && r.Status == CashRegisterStatus.Open, ct);

        if (register is null)
            return Results.NotFound(new { error = "Caixa não encontrada ou já está fechada." });

        // Verify ownership
        if (register.OperatorUserId != userId)
            return Results.Forbid();

        // Calculate expected cash amount
        // Expected = OpeningAmount + Cash sales received - Cash refunds (future)
        var cashSalesTotal = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .SelectMany(s => s.Payments)
            .Where(p => p.PaymentMethod == PaymentMethodType.Cash)
            .Sum(p => p.Amount);

        var expectedAmount = register.OpeningAmount + cashSalesTotal;
        var discrepancy = req.ClosingAmount - expectedAmount;

        // Calculate totals by payment method
        var salesByMethod = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(p => p.Amount)
            );

        var salesCount = register.Sales.Count(s => s.Status == SaleStatus.Active);
        var totalRevenue = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .Sum(s => s.TotalAmount);

        // Update register
        register.ClosedAt = DateTime.UtcNow;
        register.ClosingAmount = req.ClosingAmount;
        register.ExpectedAmount = expectedAmount;
        register.Discrepancy = discrepancy;
        register.DiscrepancyNotes = req.Notes?.Trim();
        register.Status = CashRegisterStatus.Closed;
        register.UpdatedOn = DateTime.UtcNow;
        register.UpdatedBy = userId;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new CloseRegisterResponse(
            register.ExternalId,
            register.RegisterNumber,
            register.OperatorName,
            register.OpenedAt,
            register.ClosedAt.Value,
            salesCount,
            totalRevenue,
            salesByMethod,
            expectedAmount,
            req.ClosingAmount,
            discrepancy
        ));
    }

    // ── Get Current Register ──

    /// <summary>
    /// Get the current user's open register, or null if none.
    /// </summary>
    private static async Task<IResult> GetCurrentRegister(
        HttpContext httpCtx,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var userId = GetUserId(httpCtx);

        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(r => r.OperatorUserId == userId
                                      && r.Status == CashRegisterStatus.Open, ct);

        if (register is null)
            return Results.Ok(new { open = false });

        var salesCount = register.Sales.Count(s => s.Status == SaleStatus.Active);
        var salesTotal = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .Sum(s => s.TotalAmount);

        return Results.Ok(new
        {
            open = true,
            register = new RegisterResponse(
                register.ExternalId,
                register.RegisterNumber,
                register.OperatorName,
                register.OpenedAt,
                register.ClosedAt,
                register.OpeningAmount,
                register.ClosingAmount,
                register.ExpectedAmount,
                register.Discrepancy,
                register.Status.ToString(),
                salesCount,
                salesTotal
            )
        });
    }

    // ── Get Register By ID ──

    /// <summary>
    /// Get a specific register with full details and sales summary.
    /// </summary>
    private static async Task<IResult> GetRegisterById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Payments)
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (register is null)
            return Results.NotFound(new { error = "Caixa não encontrada." });

        var activeSales = register.Sales.Where(s => s.Status == SaleStatus.Active).ToList();
        var salesCount = activeSales.Count;
        var salesTotal = activeSales.Sum(s => s.TotalAmount);
        var itemsCount = activeSales.Sum(s => s.Items.Count);

        var salesByMethod = activeSales
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(p => p.Amount)
            );

        var salesList = register.Sales
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new RegisterSaleInfo(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString()
            ))
            .ToList();

        return Results.Ok(new RegisterDetailResponse(
            register.ExternalId,
            register.RegisterNumber,
            register.OperatorUserId,
            register.OperatorName,
            register.OpenedAt,
            register.ClosedAt,
            register.OpeningAmount,
            register.ClosingAmount,
            register.ExpectedAmount,
            register.Discrepancy,
            register.DiscrepancyNotes,
            register.Status.ToString(),
            salesCount,
            salesTotal,
            itemsCount,
            salesByMethod,
            salesList,
            register.CreatedOn
        ));
    }

    // ── Get All Registers Status ──

    /// <summary>
    /// Get the status of all registers (for manager monitoring).
    /// Returns recently closed + currently open registers.
    /// </summary>
    private static async Task<IResult> GetAllRegistersStatus(
        [FromServices] ShsDbContext db,
        [FromQuery] int days = 7,
        CancellationToken ct = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-days);

        var registers = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted && s.Status == SaleStatus.Active))
            .Where(r => r.Status == CashRegisterStatus.Open || r.OpenedAt >= cutoff)
            .OrderByDescending(r => r.OpenedAt)
            .Select(r => new RegisterStatusItem(
                r.ExternalId,
                r.RegisterNumber,
                r.OperatorName,
                r.OpenedAt,
                r.ClosedAt,
                r.Status.ToString(),
                r.Sales.Count(s => s.Status == SaleStatus.Active),
                r.Sales.Where(s => s.Status == SaleStatus.Active).Sum(s => s.TotalAmount),
                r.Discrepancy
            ))
            .ToListAsync(ct);

        var openCount = registers.Count(r => r.Status == "Open");
        var closedCount = registers.Count(r => r.Status == "Closed");

        return Results.Ok(new
        {
            openCount,
            closedCount,
            registers
        });
    }
}

// ── Request DTOs ──

public record OpenRegisterRequest(decimal OpeningAmount);

public record CloseRegisterRequest(
    Guid RegisterExternalId,
    decimal ClosingAmount,
    string? Notes
);

// ── Response DTOs ──

public record RegisterResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal? ExpectedAmount,
    decimal? Discrepancy,
    string Status,
    int SalesCount,
    decimal SalesTotal
);

public record CloseRegisterResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime ClosedAt,
    int SalesCount,
    decimal TotalRevenue,
    Dictionary<string, decimal> SalesByPaymentMethod,
    decimal ExpectedCash,
    decimal CountedCash,
    decimal Discrepancy
);

public record RegisterDetailResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorUserId,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal? ExpectedAmount,
    decimal? Discrepancy,
    string? DiscrepancyNotes,
    string Status,
    int SalesCount,
    decimal SalesTotal,
    int ItemsCount,
    Dictionary<string, decimal> SalesByPaymentMethod,
    List<RegisterSaleInfo> Sales,
    DateTime CreatedOn
);

public record RegisterSaleInfo(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    int ItemCount,
    string Status
);

public record RegisterStatusItem(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string Status,
    int SalesCount,
    decimal SalesTotal,
    decimal? Discrepancy
);
