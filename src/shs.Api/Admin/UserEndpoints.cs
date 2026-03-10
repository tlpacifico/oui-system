using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users");

        group.MapGet("/", GetAll).RequirePermission("admin.users.view");
        group.MapGet("/{externalId:guid}", GetById).RequirePermission("admin.users.view");
    }

    private static async Task<IResult> GetAll(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u =>
                u.Email.Contains(search) ||
                (u.DisplayName != null && u.DisplayName.Contains(search)));

        var users = await query
            .OrderBy(u => u.Email)
            .Select(u => new
            {
                u.ExternalId,
                u.Email,
                u.DisplayName,
                u.CreatedOn,
                Roles = u.UserRoles.Select(ur => new
                {
                    ur.Role.ExternalId,
                    ur.Role.Name
                }).ToList(),
                RoleCount = u.UserRoles.Count
            })
            .ToListAsync(ct);

        return Results.Ok(users);
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users
            .Where(u => u.ExternalId == externalId)
            .Select(u => new
            {
                u.ExternalId,
                u.Email,
                u.DisplayName,
                u.CreatedOn,
                Roles = u.UserRoles.Select(ur => new
                {
                    ur.Role.ExternalId,
                    ur.Role.Name,
                    ur.AssignedOn,
                    ur.AssignedBy
                }).ToList(),
                RoleCount = u.UserRoles.Count
            })
            .FirstOrDefaultAsync(ct);

        if (user is null)
            return Results.NotFound(new { message = "Utilizador não encontrado." });

        return Results.Ok(user);
    }
}
