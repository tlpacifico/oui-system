using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/me");

        group.MapGet("/roles", GetMyRoles);
        group.MapGet("/permissions", GetMyPermissions);
    }

    private static async Task<IResult> GetMyRoles(
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var email = httpContext.User.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Results.Unauthorized();

        var roles = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.User.Email == email)
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
        var email = httpContext.User.GetUserEmail();
        if (string.IsNullOrEmpty(email))
            return Results.Unauthorized();

        var permissions = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.User.Email == email)
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
