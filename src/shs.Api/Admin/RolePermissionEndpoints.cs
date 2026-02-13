using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class RolePermissionEndpoints
{
    public static void MapRolePermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles/{roleExternalId:guid}/permissions");

        group.MapPost("/", AssignPermission).RequirePermission("admin.roles.manage-permissions");
        group.MapPost("/bulk", AssignBulkPermissions).RequirePermission("admin.roles.manage-permissions");
        group.MapDelete("/{permissionExternalId:guid}", RevokePermission).RequirePermission("admin.roles.manage-permissions");
    }

    private static async Task<IResult> AssignPermission(
        [FromRoute] Guid roleExternalId,
        [FromBody] AssignPermissionRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == roleExternalId, ct);
        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.ExternalId == request.PermissionExternalId, ct);
        if (permission is null)
            return Results.NotFound(new { message = "Permission not found." });

        // Check if already assigned
        var exists = await db.RolePermissions.AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, ct);
        if (exists)
            return Results.BadRequest(new { message = "Permission already assigned to role." });

        var rolePermission = new RolePermissionEntity
        {
            RoleId = role.Id,
            PermissionId = permission.Id,
            GrantedOn = DateTime.UtcNow,
            GrantedBy = httpContext.User.GetUserEmail()
        };

        db.RolePermissions.Add(rolePermission);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Permission assigned successfully." });
    }

    private static async Task<IResult> AssignBulkPermissions(
        [FromRoute] Guid roleExternalId,
        [FromBody] AssignBulkPermissionsRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.ExternalId == roleExternalId, ct);

        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        var permissions = await db.Permissions
            .Where(p => request.PermissionExternalIds.Contains(p.ExternalId))
            .ToListAsync(ct);

        if (permissions.Count != request.PermissionExternalIds.Count)
            return Results.BadRequest(new { message = "Some permissions not found." });

        var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var newRolePermissions = permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .Select(p => new RolePermissionEntity
            {
                RoleId = role.Id,
                PermissionId = p.Id,
                GrantedOn = DateTime.UtcNow,
                GrantedBy = httpContext.User.GetUserEmail()
            })
            .ToList();

        if (newRolePermissions.Any())
        {
            db.RolePermissions.AddRange(newRolePermissions);
            await db.SaveChangesAsync(ct);
        }

        return Results.Ok(new { message = $"{newRolePermissions.Count} permission(s) assigned successfully." });
    }

    private static async Task<IResult> RevokePermission(
        [FromRoute] Guid roleExternalId,
        [FromRoute] Guid permissionExternalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == roleExternalId, ct);
        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.ExternalId == permissionExternalId, ct);
        if (permission is null)
            return Results.NotFound(new { message = "Permission not found." });

        var rolePermission = await db.RolePermissions
            .FirstOrDefaultAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, ct);

        if (rolePermission is null)
            return Results.NotFound(new { message = "Permission not assigned to role." });

        db.RolePermissions.Remove(rolePermission);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Permission revoked successfully." });
    }
}

public record AssignPermissionRequest(Guid PermissionExternalId);
public record AssignBulkPermissionsRequest(List<Guid> PermissionExternalIds);
