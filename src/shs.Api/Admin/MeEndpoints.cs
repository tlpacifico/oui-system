using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me").RequireAuthorization();

        group.MapGet("/roles", GetMyRoles);
        group.MapGet("/permissions", GetMyPermissions);
    }

    private static async Task<IResult> GetMyRoles(
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var userId = httpContext.User.GetUserId();
        if (userId == 0)
            return Results.Unauthorized();

        var roles = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .Select(ur => new { ur.Role.Name })
            .ToListAsync(ct);

        return Results.Ok(roles);
    }

    private static async Task<IResult> GetMyPermissions(
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var userId = httpContext.User.GetUserId();
        if (userId == 0)
            return Results.Unauthorized();

        var permissions = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
            .Distinct()
            .Select(p => new { p.Name })
            .ToListAsync(ct);

        return Results.Ok(permissions);
    }
}
