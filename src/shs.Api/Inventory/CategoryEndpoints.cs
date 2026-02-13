using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Inventory;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories")
            .RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{externalId:guid}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{externalId:guid}", Update);
        group.MapDelete("/{externalId:guid}", Delete);
    }

    private static async Task<IResult> GetAll(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var query = db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s));
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListResponse(
                c.ExternalId,
                c.Name,
                c.Description,
                c.ParentCategoryId.HasValue
                    ? new CategoryParentInfo(c.ParentCategory!.ExternalId, c.ParentCategory.Name)
                    : null,
                c.SubCategories.Count(sc => !sc.IsDeleted),
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var category = await db.Categories
            .Where(c => c.ExternalId == externalId)
            .Select(c => new CategoryDetailResponse(
                c.ExternalId,
                c.Name,
                c.Description,
                c.ParentCategoryId.HasValue
                    ? new CategoryParentInfo(c.ParentCategory!.ExternalId, c.ParentCategory.Name)
                    : null,
                c.SubCategories
                    .Where(sc => !sc.IsDeleted)
                    .Select(sc => new CategoryChildInfo(sc.ExternalId, sc.Name))
                    .ToList(),
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn,
                c.CreatedBy,
                c.UpdatedOn,
                c.UpdatedBy
            ))
            .FirstOrDefaultAsync(ct);

        return category is null
            ? Results.NotFound(new { error = "Category not found" })
            : Results.Ok(category);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateCategoryRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 200)
            return Results.BadRequest(new { error = "Name must be at most 200 characters." });

        // Check duplicate name
        var nameExists = await db.Categories
            .AnyAsync(c => c.Name.ToLower() == req.Name.Trim().ToLower(), ct);

        if (nameExists)
            return Results.Conflict(new { error = "A category with this name already exists." });

        // Validate parent category if provided
        long? parentId = null;
        if (req.ParentCategoryExternalId.HasValue)
        {
            var parent = await db.Categories
                .FirstOrDefaultAsync(c => c.ExternalId == req.ParentCategoryExternalId.Value, ct);

            if (parent is null)
                return Results.BadRequest(new { error = "Parent category not found." });

            parentId = parent.Id;
        }

        var category = new CategoryEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            ParentCategoryId = parentId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        // Load parent for response
        CategoryParentInfo? parentInfo = null;
        if (parentId.HasValue)
        {
            var parent = await db.Categories
                .Where(c => c.Id == parentId.Value)
                .Select(c => new CategoryParentInfo(c.ExternalId, c.Name))
                .FirstOrDefaultAsync(ct);
            parentInfo = parent;
        }

        return Results.Created(
            $"/api/categories/{category.ExternalId}",
            new CategoryDetailResponse(
                category.ExternalId,
                category.Name,
                category.Description,
                parentInfo,
                new List<CategoryChildInfo>(),
                0,
                category.CreatedOn,
                category.CreatedBy,
                category.UpdatedOn,
                category.UpdatedBy
            )
        );
    }

    private static async Task<IResult> Update(
        Guid externalId,
        [FromBody] UpdateCategoryRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == externalId, ct);

        if (category is null)
            return Results.NotFound(new { error = "Category not found" });

        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 200)
            return Results.BadRequest(new { error = "Name must be at most 200 characters." });

        // Check duplicate name (excluding current)
        var nameExists = await db.Categories
            .AnyAsync(c => c.Name.ToLower() == req.Name.Trim().ToLower() && c.Id != category.Id, ct);

        if (nameExists)
            return Results.Conflict(new { error = "A category with this name already exists." });

        // Validate parent category if provided
        long? parentId = null;
        if (req.ParentCategoryExternalId.HasValue)
        {
            var parent = await db.Categories
                .FirstOrDefaultAsync(c => c.ExternalId == req.ParentCategoryExternalId.Value, ct);

            if (parent is null)
                return Results.BadRequest(new { error = "Parent category not found." });

            // Prevent circular reference
            if (parent.Id == category.Id)
                return Results.BadRequest(new { error = "A category cannot be its own parent." });

            parentId = parent.Id;
        }

        category.Name = req.Name.Trim();
        category.Description = req.Description?.Trim();
        category.ParentCategoryId = parentId;
        category.UpdatedOn = DateTime.UtcNow;
        category.UpdatedBy = "system";

        await db.SaveChangesAsync(ct);

        // Load full response
        var response = await db.Categories
            .Where(c => c.Id == category.Id)
            .Select(c => new CategoryDetailResponse(
                c.ExternalId,
                c.Name,
                c.Description,
                c.ParentCategoryId.HasValue
                    ? new CategoryParentInfo(c.ParentCategory!.ExternalId, c.ParentCategory.Name)
                    : null,
                c.SubCategories
                    .Where(sc => !sc.IsDeleted)
                    .Select(sc => new CategoryChildInfo(sc.ExternalId, sc.Name))
                    .ToList(),
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn,
                c.CreatedBy,
                c.UpdatedOn,
                c.UpdatedBy
            ))
            .FirstAsync(ct);

        return Results.Ok(response);
    }

    private static async Task<IResult> Delete(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == externalId, ct);

        if (category is null)
            return Results.NotFound(new { error = "Category not found" });

        // Check if category has items
        var hasItems = await db.Items.AnyAsync(i => i.CategoryId == category.Id && !i.IsDeleted, ct);
        if (hasItems)
            return Results.Conflict(new { error = "Cannot delete a category that has items assigned to it." });

        // Check if category has subcategories
        var hasChildren = await db.Categories.AnyAsync(c => c.ParentCategoryId == category.Id && !c.IsDeleted, ct);
        if (hasChildren)
            return Results.Conflict(new { error = "Cannot delete a category that has subcategories." });

        // Soft delete
        category.IsDeleted = true;
        category.DeletedBy = "system";
        category.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}

// Request DTOs
public record CreateCategoryRequest(string Name, string? Description, Guid? ParentCategoryExternalId);
public record UpdateCategoryRequest(string Name, string? Description, Guid? ParentCategoryExternalId);

// Response DTOs
public record CategoryListResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    CategoryParentInfo? ParentCategory,
    int SubCategoryCount,
    int ItemCount,
    DateTime CreatedOn
);

public record CategoryDetailResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    CategoryParentInfo? ParentCategory,
    List<CategoryChildInfo> SubCategories,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);

public record CategoryParentInfo(Guid ExternalId, string Name);
public record CategoryChildInfo(Guid ExternalId, string Name);
