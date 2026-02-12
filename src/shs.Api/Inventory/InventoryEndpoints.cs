using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;
using shs.Infrastructure.Services;

namespace shs.Api.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory")
            .WithTags("Inventory")
            .RequireAuthorization();

        group.MapPost("/items/consignment", CreateConsignmentItem);
        group.MapGet("/items", GetItems);
        group.MapGet("/items/{externalId:guid}", GetItemById);
    }

    private static async Task<IResult> CreateConsignmentItem(
        [FromBody] CreateConsignmentItemRequest req,
        [FromServices] ShsDbContext db,
        [FromServices] IItemIdGeneratorService idGenerator,
        CancellationToken ct)
    {
        // Validate reception exists
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.ExternalId == req.ReceptionExternalId, ct);

        if (reception == null)
            return Results.NotFound(new { error = "Reception not found" });

        // Validate brand exists
        var brandExists = await db.Brands.AnyAsync(b => b.Id == req.BrandId, ct);
        if (!brandExists)
            return Results.BadRequest(new { error = "Brand not found" });

        // Validate category if provided
        if (req.CategoryId.HasValue)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == req.CategoryId.Value, ct);
            if (!categoryExists)
                return Results.BadRequest(new { error = "Category not found" });
        }

        // Validate condition
        if (!Enum.TryParse<ItemCondition>(req.Condition, out var condition))
            return Results.BadRequest(new { error = "Invalid condition value" });

        // Validate tags exist
        var tagIds = req.TagIds.ToList();
        if (tagIds.Any())
        {
            var existingTags = await db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync(ct);

            if (existingTags.Count != tagIds.Count)
                return Results.BadRequest(new { error = "One or more tags not found" });
        }

        // Generate ID
        var itemId = await idGenerator.GenerateNextIdAsync(reception.SupplierId, ct);

        // Create item
        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = req.Name,
            Description = req.Description,
            BrandId = req.BrandId,
            CategoryId = req.CategoryId,
            Size = req.Size,
            Color = req.Color,
            Composition = req.Composition,
            Condition = condition,
            EvaluatedPrice = req.EvaluatedPrice,
            Status = req.IsRejected ? ItemStatus.Rejected : ItemStatus.Evaluated,
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = reception.SupplierId,
            ReceptionId = reception.Id,
            CommissionPercentage = 50m, // Default 50%
            IsRejected = req.IsRejected,
            RejectionReason = req.RejectionReason,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        // Add tags
        if (tagIds.Any())
        {
            var tags = await db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync(ct);
            item.Tags = tags;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/inventory/items/{item.ExternalId}",
            new CreateConsignmentItemResponse(
                item.ExternalId,
                item.IdentificationNumber,
                item.Name,
                item.Status.ToString(),
                item.CreatedOn
            )
        );
    }

    private static async Task<IResult> GetItems(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search,
        [FromQuery] long? brandId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Items
            .Include(i => i.Brand)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(s) ||
                i.IdentificationNumber.ToLower().Contains(s));
        }

        if (brandId.HasValue)
            query = query.Where(i => i.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ItemStatus>(status, out var itemStatus))
            query = query.Where(i => i.Status == itemStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemListItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.ThumbnailPath ?? p.FilePath).FirstOrDefault(),
                i.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<ItemListItemResponse>(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        ));
    }

    private static async Task<IResult> GetItemById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .Include(i => i.Tags)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item == null)
            return Results.NotFound();

        return Results.Ok(new ItemDetailResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Description,
            new BrandInfo(item.Brand.Id, item.Brand.Name),
            item.Category != null ? new CategoryInfo(item.Category.Id, item.Category.Name) : null,
            item.Size,
            item.Color,
            item.Composition,
            item.Condition.ToString(),
            item.EvaluatedPrice,
            item.CostPrice,
            item.FinalSalePrice,
            item.Status.ToString(),
            item.AcquisitionType.ToString(),
            item.Origin.ToString(),
            item.Supplier != null ? new SupplierInfo(item.Supplier.Id, item.Supplier.Name) : null,
            item.CommissionPercentage,
            item.CommissionAmount,
            item.IsRejected,
            item.RejectionReason,
            item.SoldAt,
            item.DaysInStock,
            item.Tags.Select(t => new TagInfo(t.Id, t.Name, t.Color)).ToList(),
            item.Photos.Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary)).ToList(),
            item.CreatedOn,
            item.CreatedBy,
            item.UpdatedOn,
            item.UpdatedBy
        ));
    }
}

// Request DTOs
public record CreateConsignmentItemRequest(
    Guid ReceptionExternalId,
    string Name,
    string? Description,
    long BrandId,
    long? CategoryId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    long[] TagIds,
    bool IsRejected,
    string? RejectionReason
);

// Response DTOs
public record CreateConsignmentItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Status,
    DateTime CreatedAt
);

public record ItemListItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    decimal EvaluatedPrice,
    string Status,
    string? PrimaryPhotoUrl,
    DateTime CreatedOn
);

public record ItemDetailResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string? Description,
    BrandInfo Brand,
    CategoryInfo? Category,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    decimal? FinalSalePrice,
    string Status,
    string AcquisitionType,
    string Origin,
    SupplierInfo? Supplier,
    decimal CommissionPercentage,
    decimal? CommissionAmount,
    bool IsRejected,
    string? RejectionReason,
    DateTime? SoldAt,
    int DaysInStock,
    List<TagInfo> Tags,
    List<PhotoInfo> Photos,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record BrandInfo(long Id, string Name);
public record CategoryInfo(long Id, string Name);
public record SupplierInfo(long Id, string Name);
public record TagInfo(long Id, string Name, string? Color);
public record PhotoInfo(Guid ExternalId, string FilePath, string? ThumbnailPath, int DisplayOrder, bool IsPrimary);
