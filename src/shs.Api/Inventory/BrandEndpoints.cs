using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Inventory;

public static class BrandEndpoints
{
    public static void MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/brands")
            .WithTags("Brands")
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
        var query = db.Brands.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(s));
        }

        var brands = await query
            .OrderBy(b => b.Name)
            .Select(b => new BrandListResponse(
                b.ExternalId,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.Items.Count(i => !i.IsDeleted),
                b.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(brands);
    }

    private static async Task<IResult> GetById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var brand = await db.Brands
            .Where(b => b.ExternalId == externalId)
            .Select(b => new BrandDetailResponse(
                b.ExternalId,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.Items.Count(i => !i.IsDeleted),
                b.CreatedOn,
                b.CreatedBy,
                b.UpdatedOn,
                b.UpdatedBy
            ))
            .FirstOrDefaultAsync(ct);

        return brand is null
            ? Results.NotFound(new { error = "Brand not found" })
            : Results.Ok(brand);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateBrandRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 200)
            return Results.BadRequest(new { error = "Name must be at most 200 characters." });

        // Check for duplicate name
        var nameExists = await db.Brands
            .AnyAsync(b => b.Name.ToLower() == req.Name.Trim().ToLower(), ct);

        if (nameExists)
            return Results.Conflict(new { error = "A brand with this name already exists." });

        var brand = new BrandEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            LogoUrl = req.LogoUrl?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        db.Brands.Add(brand);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/brands/{brand.ExternalId}",
            new BrandDetailResponse(
                brand.ExternalId,
                brand.Name,
                brand.Description,
                brand.LogoUrl,
                0,
                brand.CreatedOn,
                brand.CreatedBy,
                brand.UpdatedOn,
                brand.UpdatedBy
            )
        );
    }

    private static async Task<IResult> Update(
        Guid externalId,
        [FromBody] UpdateBrandRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var brand = await db.Brands
            .FirstOrDefaultAsync(b => b.ExternalId == externalId, ct);

        if (brand is null)
            return Results.NotFound(new { error = "Brand not found" });

        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 200)
            return Results.BadRequest(new { error = "Name must be at most 200 characters." });

        // Check duplicate name (excluding current brand)
        var nameExists = await db.Brands
            .AnyAsync(b => b.Name.ToLower() == req.Name.Trim().ToLower() && b.Id != brand.Id, ct);

        if (nameExists)
            return Results.Conflict(new { error = "A brand with this name already exists." });

        brand.Name = req.Name.Trim();
        brand.Description = req.Description?.Trim();
        brand.LogoUrl = req.LogoUrl?.Trim();
        brand.UpdatedOn = DateTime.UtcNow;
        brand.UpdatedBy = "system"; // TODO: Get from JWT claims

        await db.SaveChangesAsync(ct);

        var itemCount = await db.Items.CountAsync(i => i.BrandId == brand.Id && !i.IsDeleted, ct);

        return Results.Ok(new BrandDetailResponse(
            brand.ExternalId,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            itemCount,
            brand.CreatedOn,
            brand.CreatedBy,
            brand.UpdatedOn,
            brand.UpdatedBy
        ));
    }

    private static async Task<IResult> Delete(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var brand = await db.Brands
            .FirstOrDefaultAsync(b => b.ExternalId == externalId, ct);

        if (brand is null)
            return Results.NotFound(new { error = "Brand not found" });

        // Check if brand has items
        var hasItems = await db.Items.AnyAsync(i => i.BrandId == brand.Id && !i.IsDeleted, ct);
        if (hasItems)
            return Results.Conflict(new { error = "Cannot delete a brand that has items assigned to it." });

        // Soft delete
        brand.IsDeleted = true;
        brand.DeletedBy = "system"; // TODO: Get from JWT claims
        brand.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }
}

// Request DTOs
public record CreateBrandRequest(string Name, string? Description, string? LogoUrl);
public record UpdateBrandRequest(string Name, string? Description, string? LogoUrl);

// Response DTOs
public record BrandListResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    string? LogoUrl,
    int ItemCount,
    DateTime CreatedOn
);

public record BrandDetailResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    string? LogoUrl,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);
