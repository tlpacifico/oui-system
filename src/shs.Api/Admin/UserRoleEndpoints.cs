using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class UserRoleEndpoints
{
    public static void MapUserRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users/{userExternalId:guid}/roles");

        group.MapGet("/", GetUserRoles).RequirePermission("admin.users.view");
        group.MapPost("/", AssignRole).RequirePermission("admin.users.manage-roles");
        group.MapPost("/bulk", AssignBulkRoles).RequirePermission("admin.users.manage-roles");
        group.MapDelete("/{roleExternalId:guid}", RevokeRole).RequirePermission("admin.users.manage-roles");
    }

    private static async Task<IResult> GetUserRoles(
        [FromRoute] Guid userExternalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == userExternalId, ct);
        if (user is null)
            return Results.NotFound(new { message = "User not found." });

        var roles = await db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => new
            {
                RoleId = ur.Role.ExternalId,
                ur.Role.Name,
                ur.Role.Description,
                ur.AssignedOn,
                ur.AssignedBy
            })
            .ToListAsync(ct);

        return Results.Ok(roles);
    }

    private static async Task<IResult> AssignRole(
        [FromRoute] Guid userExternalId,
        [FromBody] AssignRoleRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == userExternalId, ct);
        if (user is null)
            return Results.NotFound(new { message = "User not found." });

        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == request.RoleExternalId, ct);
        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        // Check if already assigned
        var exists = await db.UserRoles.AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, ct);
        if (exists)
            return Results.BadRequest(new { message = "Role already assigned to user." });

        var userRole = new UserRoleEntity
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedOn = DateTime.UtcNow,
            AssignedBy = httpContext.User.GetUserEmail()
        };

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Role assigned successfully." });
    }

    private static async Task<IResult> AssignBulkRoles(
        [FromRoute] Guid userExternalId,
        [FromBody] AssignBulkRolesRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.ExternalId == userExternalId, ct);

        if (user is null)
            return Results.NotFound(new { message = "User not found." });

        var roles = await db.Roles
            .Where(r => request.RoleExternalIds.Contains(r.ExternalId))
            .ToListAsync(ct);

        if (roles.Count != request.RoleExternalIds.Count)
            return Results.BadRequest(new { message = "Some roles not found." });

        var existingRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var newUserRoles = roles
            .Where(r => !existingRoleIds.Contains(r.Id))
            .Select(r => new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = r.Id,
                AssignedOn = DateTime.UtcNow,
                AssignedBy = httpContext.User.GetUserEmail()
            })
            .ToList();

        if (newUserRoles.Any())
        {
            db.UserRoles.AddRange(newUserRoles);
            await db.SaveChangesAsync(ct);
        }

        return Results.Ok(new { message = $"{newUserRoles.Count} role(s) assigned successfully." });
    }

    private static async Task<IResult> RevokeRole(
        [FromRoute] Guid userExternalId,
        [FromRoute] Guid roleExternalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == userExternalId, ct);
        if (user is null)
            return Results.NotFound(new { message = "User not found." });

        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == roleExternalId, ct);
        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, ct);

        if (userRole is null)
            return Results.NotFound(new { message = "Role not assigned to user." });

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Role revoked successfully." });
    }
}

public record AssignRoleRequest(Guid RoleExternalId);
public record AssignBulkRolesRequest(List<Guid> RoleExternalIds);
