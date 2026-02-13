using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class PermissionEndpoints
{
    public static void MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/permissions");

        group.MapGet("/", GetAll).RequirePermission("admin.permissions.view");
        group.MapGet("/categories", GetCategories).RequirePermission("admin.permissions.view");
    }

    private static async Task<IResult> GetAll(
        [FromServices] ShsDbContext db,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = db.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Description != null && p.Description.Contains(search)));

        var permissions = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new
            {
                p.ExternalId,
                p.Name,
                p.Category,
                p.Description
            })
            .ToListAsync(ct);

        return Results.Ok(permissions);
    }

    private static async Task<IResult> GetCategories(
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var categories = await db.Permissions
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return Results.Ok(categories);
    }
}
