using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Consignment;

public static class SupplierReturnEndpoints
{
    public static void MapSupplierReturnEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/consignments/returns").WithTags("SupplierReturns");

        group.MapGet("/", GetReturns).RequirePermission("consignment.returns.view");
        group.MapGet("/{externalId:guid}", GetReturnById).RequirePermission("consignment.returns.view");
        group.MapPost("/", CreateReturn).RequirePermission("consignment.returns.create");
        group.MapGet("/returnable-items", GetReturnableItems).RequirePermission("consignment.returns.view");
    }

    /// <summary>
    /// List items that can be returned to a specific supplier.
    /// Returnable items are those with status ToSell or Rejected (not yet sold/returned).
    /// </summary>
    private static async Task<IResult> GetReturnableItems(
        [FromQuery] Guid supplierExternalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplier = await db.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ExternalId == supplierExternalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        var items = await db.Items
            .Where(i => i.SupplierId == supplier.Id
                        && !i.IsDeleted
                        && (i.Status == ItemStatus.ToSell || i.Status == ItemStatus.Rejected))
            .Include(i => i.Brand)
            .Include(i => i.Photos.Where(p => p.IsPrimary))
            .OrderBy(i => i.IdentificationNumber)
            .Select(i => new ReturnableItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.IsRejected,
                i.DaysInStock,
                i.Photos.Where(p => p.IsPrimary).Select(p => p.FilePath).FirstOrDefault(),
                i.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(items);
    }

    /// <summary>
    /// Create a new supplier return — marks selected items as Returned.
    /// </summary>
    private static async Task<IResult> CreateReturn(
        [FromBody] CreateSupplierReturnRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (req.ItemExternalIds is null || req.ItemExternalIds.Length == 0)
            return Results.BadRequest(new { error = "Selecione pelo menos um item para devolver." });

        // Validate supplier
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == req.SupplierExternalId, ct);

        if (supplier is null)
            return Results.NotFound(new { error = "Fornecedor não encontrado." });

        // Load items to return
        var items = await db.Items
            .Where(i => req.ItemExternalIds.Contains(i.ExternalId)
                        && i.SupplierId == supplier.Id
                        && !i.IsDeleted)
            .Include(i => i.Brand)
            .ToListAsync(ct);

        if (items.Count == 0)
            return Results.BadRequest(new { error = "Nenhum item válido encontrado." });

        // Validate all items are returnable
        var nonReturnable = items
            .Where(i => i.Status != ItemStatus.ToSell && i.Status != ItemStatus.Rejected)
            .ToList();

        if (nonReturnable.Count > 0)
        {
            var ids = string.Join(", ", nonReturnable.Select(i => i.IdentificationNumber));
            return Results.BadRequest(new { error = $"Os seguintes itens não podem ser devolvidos (estado inválido): {ids}" });
        }

        // Create the return record
        var supplierReturn = new SupplierReturnEntity
        {
            ExternalId = Guid.NewGuid(),
            SupplierId = supplier.Id,
            ReturnDate = DateTime.UtcNow,
            ItemCount = items.Count,
            Notes = req.Notes?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        db.SupplierReturns.Add(supplierReturn);

        // Mark items as returned
        foreach (var item in items)
        {
            item.Status = ItemStatus.Returned;
            item.SupplierReturnId = supplierReturn.Id;
            item.ReturnedAt = DateTime.UtcNow;
            item.UpdatedOn = DateTime.UtcNow;
            item.UpdatedBy = "system";
        }

        // Need to save first to get SupplierReturn.Id, but we can use the entity tracking
        // Actually, EF Core will fix up the FK on SaveChanges since we add items to the nav property
        // Let's just set the navigation instead:
        supplierReturn.Items = items;

        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/consignments/returns/{supplierReturn.ExternalId}",
            new SupplierReturnDetailResponse(
                supplierReturn.ExternalId,
                new ReturnSupplierInfo(supplier.ExternalId, supplier.Name, supplier.Initial),
                supplierReturn.ReturnDate,
                supplierReturn.ItemCount,
                supplierReturn.Notes,
                supplierReturn.CreatedOn,
                supplierReturn.CreatedBy,
                items.Select(i => new ReturnItemResponse(
                    i.ExternalId,
                    i.IdentificationNumber,
                    i.Name,
                    i.Brand.Name,
                    i.Size,
                    i.Color,
                    i.Condition.ToString(),
                    i.EvaluatedPrice,
                    i.IsRejected
                )).ToList()
            )
        );
    }

    /// <summary>
    /// List all supplier returns with pagination.
    /// </summary>
    private static async Task<IResult> GetReturns(
        [FromServices] ShsDbContext db,
        [FromQuery] Guid? supplierExternalId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.SupplierReturns
            .Include(r => r.Supplier)
            .AsQueryable();

        if (supplierExternalId.HasValue)
        {
            var supplier = await db.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ExternalId == supplierExternalId.Value, ct);

            if (supplier is not null)
                query = query.Where(r => r.SupplierId == supplier.Id);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Supplier.Name.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync(ct);

        var returns = await query
            .OrderByDescending(r => r.ReturnDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new SupplierReturnListItemResponse(
                r.ExternalId,
                new ReturnSupplierInfo(r.Supplier.ExternalId, r.Supplier.Name, r.Supplier.Initial),
                r.ReturnDate,
                r.ItemCount,
                r.Notes,
                r.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(new SupplierReturnPagedResult(
            returns,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        ));
    }

    /// <summary>
    /// Get a single return with its items.
    /// </summary>
    private static async Task<IResult> GetReturnById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var supplierReturn = await db.SupplierReturns
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
                .ThenInclude(i => i.Brand)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (supplierReturn is null)
            return Results.NotFound(new { error = "Devolução não encontrada." });

        return Results.Ok(new SupplierReturnDetailResponse(
            supplierReturn.ExternalId,
            new ReturnSupplierInfo(
                supplierReturn.Supplier.ExternalId,
                supplierReturn.Supplier.Name,
                supplierReturn.Supplier.Initial),
            supplierReturn.ReturnDate,
            supplierReturn.ItemCount,
            supplierReturn.Notes,
            supplierReturn.CreatedOn,
            supplierReturn.CreatedBy,
            supplierReturn.Items.Select(i => new ReturnItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.Condition.ToString(),
                i.EvaluatedPrice,
                i.IsRejected
            )).ToList()
        ));
    }
}

// ── Request DTOs ──

public record CreateSupplierReturnRequest(
    Guid SupplierExternalId,
    Guid[] ItemExternalIds,
    string? Notes
);

// ── Response DTOs ──

public record ReturnSupplierInfo(
    Guid ExternalId,
    string Name,
    string Initial
);

public record ReturnableItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    string Status,
    bool IsRejected,
    int DaysInStock,
    string? PrimaryPhotoUrl,
    DateTime CreatedOn
);

public record ReturnItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    bool IsRejected
);

public record SupplierReturnListItemResponse(
    Guid ExternalId,
    ReturnSupplierInfo Supplier,
    DateTime ReturnDate,
    int ItemCount,
    string? Notes,
    DateTime CreatedOn
);

public record SupplierReturnDetailResponse(
    Guid ExternalId,
    ReturnSupplierInfo Supplier,
    DateTime ReturnDate,
    int ItemCount,
    string? Notes,
    DateTime CreatedOn,
    string? CreatedBy,
    List<ReturnItemResponse> Items
);

public record SupplierReturnPagedResult(
    List<SupplierReturnListItemResponse> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
