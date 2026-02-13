using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Admin;

public static class RoleEndpoints
{
    public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/roles");

        group.MapGet("/", GetAll).RequirePermission("admin.roles.view");
        group.MapGet("/{externalId:guid}", GetById).RequirePermission("admin.roles.view");
        group.MapPost("/", Create).RequirePermission("admin.roles.create");
        group.MapPut("/{externalId:guid}", Update).RequirePermission("admin.roles.update");
        group.MapDelete("/{externalId:guid}", Delete).RequirePermission("admin.roles.delete");
    }

    private static async Task<IResult> GetAll(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var query = db.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Name.Contains(search) || (r.Description != null && r.Description.Contains(search)));

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.ExternalId,
                r.Name,
                r.Description,
                r.IsSystemRole,
                UserCount = r.UserRoles.Count,
                PermissionCount = r.RolePermissions.Count,
                r.CreatedOn,
                r.CreatedBy,
                r.UpdatedOn,
                r.UpdatedBy
            })
            .ToListAsync(ct);

        return Results.Ok(roles);
    }

    private static async Task<IResult> GetById(
        [FromRoute] Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles
            .Where(r => r.ExternalId == externalId)
            .Select(r => new
            {
                r.ExternalId,
                r.Name,
                r.Description,
                r.IsSystemRole,
                UserCount = r.UserRoles.Count,
                PermissionCount = r.RolePermissions.Count,
                r.CreatedOn,
                r.CreatedBy,
                r.UpdatedOn,
                r.UpdatedBy,
                Permissions = r.RolePermissions.Select(rp => new
                {
                    rp.Permission.ExternalId,
                    rp.Permission.Name,
                    rp.Permission.Category,
                    rp.Permission.Description
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        return Results.Ok(role);
    }

    private static async Task<IResult> Create(
        [FromBody] CreateRoleRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        // Check if role name already exists
        if (await db.Roles.AnyAsync(r => r.Name == request.Name, ct))
            return Results.BadRequest(new { message = "Role name already exists." });

        var role = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = httpContext.User.GetUserEmail()
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/roles/{role.ExternalId}", new { role.ExternalId });
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid externalId,
        [FromBody] UpdateRoleRequest request,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);
        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        if (role.IsSystemRole)
            return Results.BadRequest(new { message = "Cannot modify system roles." });

        // Check if new name already exists (excluding current role)
        if (request.Name != role.Name && await db.Roles.AnyAsync(r => r.Name == request.Name, ct))
            return Results.BadRequest(new { message = "Role name already exists." });

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedOn = DateTime.UtcNow;
        role.UpdatedBy = httpContext.User.GetUserEmail();

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Role updated successfully." });
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid externalId,
        HttpContext httpContext,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var role = await db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.ExternalId == externalId, ct);

        if (role is null)
            return Results.NotFound(new { message = "Role not found." });

        if (role.IsSystemRole)
            return Results.BadRequest(new { message = "Cannot delete system roles." });

        if (role.UserRoles.Any())
            return Results.BadRequest(new { message = "Cannot delete role with assigned users." });

        role.IsDeleted = true;
        role.DeletedOn = DateTime.UtcNow;
        role.DeletedBy = httpContext.User.GetUserEmail();

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Role deleted successfully." });
    }
}

public record CreateRoleRequest(string Name, string? Description);
public record UpdateRoleRequest(string Name, string? Description);
