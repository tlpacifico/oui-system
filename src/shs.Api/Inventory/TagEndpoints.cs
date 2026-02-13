using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Inventory;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tags")
            .WithTags("Tags")
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
        var query = db.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Select(t => new TagListResponse(
                t.ExternalId,
                t.Name,
                t.Color,
                t.Items.Count(i => !i.IsDeleted),
                t.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(tags);
    }

    private static async Task<IResult> GetById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var tag = await db.Tags
            .Where(t => t.ExternalId == externalId)
            .Select(t => new TagDetailResponse(
                t.ExternalId,
                t.Name,
                t.Color,
                t.Items.Count(i => !i.IsDeleted),
                t.CreatedOn,
                t.CreatedBy,
                t.UpdatedOn,
                t.UpdatedBy
            ))
            .FirstOrDefaultAsync(ct);

        return tag is null
            ? Results.NotFound(new { error = "Tag not found" })
            : Results.Ok(tag);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateTagRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 100)
            return Results.BadRequest(new { error = "Name must be at most 100 characters." });

        // Validate color format if provided
        if (req.Color is not null && !IsValidHexColor(req.Color))
            return Results.BadRequest(new { error = "Color must be a valid hex color (e.g. #FF5733)." });

        // Check duplicate name
        var nameExists = await db.Tags
            .AnyAsync(t => t.Name.ToLower() == req.Name.Trim().ToLower(), ct);

        if (nameExists)
            return Results.Conflict(new { error = "A tag with this name already exists." });

        var tag = new TagEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = req.Name.Trim(),
            Color = req.Color?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/tags/{tag.ExternalId}",
            new TagDetailResponse(
                tag.ExternalId,
                tag.Name,
                tag.Color,
                0,
                tag.CreatedOn,
                tag.CreatedBy,
                tag.UpdatedOn,
                tag.UpdatedBy
            )
        );
    }

    private static async Task<IResult> Update(
        Guid externalId,
        [FromBody] UpdateTagRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var tag = await db.Tags
            .FirstOrDefaultAsync(t => t.ExternalId == externalId, ct);

        if (tag is null)
            return Results.NotFound(new { error = "Tag not found" });

        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "Name is required." });

        if (req.Name.Length > 100)
            return Results.BadRequest(new { error = "Name must be at most 100 characters." });

        if (req.Color is not null && !IsValidHexColor(req.Color))
            return Results.BadRequest(new { error = "Color must be a valid hex color (e.g. #FF5733)." });

        // Check duplicate name (excluding current)
        var nameExists = await db.Tags
            .AnyAsync(t => t.Name.ToLower() == req.Name.Trim().ToLower() && t.Id != tag.Id, ct);

        if (nameExists)
            return Results.Conflict(new { error = "A tag with this name already exists." });

        tag.Name = req.Name.Trim();
        tag.Color = req.Color?.Trim();
        tag.UpdatedOn = DateTime.UtcNow;
        tag.UpdatedBy = "system";

        await db.SaveChangesAsync(ct);

        var itemCount = await db.Items
            .CountAsync(i => i.Tags.Any(t => t.Id == tag.Id) && !i.IsDeleted, ct);

        return Results.Ok(new TagDetailResponse(
            tag.ExternalId,
            tag.Name,
            tag.Color,
            itemCount,
            tag.CreatedOn,
            tag.CreatedBy,
            tag.UpdatedOn,
            tag.UpdatedBy
        ));
    }

    private static async Task<IResult> Delete(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var tag = await db.Tags
            .FirstOrDefaultAsync(t => t.ExternalId == externalId, ct);

        if (tag is null)
            return Results.NotFound(new { error = "Tag not found" });

        // Check if tag has items
        var hasItems = await db.Items.AnyAsync(i => i.Tags.Any(t => t.Id == tag.Id) && !i.IsDeleted, ct);
        if (hasItems)
            return Results.Conflict(new { error = "Cannot delete a tag that has items assigned to it. Remove the tag from items first." });

        // Soft delete
        tag.IsDeleted = true;
        tag.DeletedBy = "system";
        tag.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    private static bool IsValidHexColor(string color)
    {
        if (string.IsNullOrWhiteSpace(color)) return false;
        color = color.Trim();
        if (!color.StartsWith('#')) return false;
        if (color.Length != 7) return false;
        return color[1..].All(c => "0123456789abcdefABCDEF".Contains(c));
    }
}

// Request DTOs
public record CreateTagRequest(string Name, string? Color);
public record UpdateTagRequest(string Name, string? Color);

// Response DTOs
public record TagListResponse(
    Guid ExternalId,
    string Name,
    string? Color,
    int ItemCount,
    DateTime CreatedOn
);

public record TagDetailResponse(
    Guid ExternalId,
    string Name,
    string? Color,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);
