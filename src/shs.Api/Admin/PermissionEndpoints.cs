using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using Oui.Modules.Auth.Infrastructure;

namespace shs.Api.Admin;

public static class PermissionEndpoints
{
    public static void MapPermissionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/permissions");

        group.MapGet("/", GetAll).RequirePermission("admin.permissions.view");
        group.MapGet("/categories", GetCategories).RequirePermission("admin.permissions.view");
        group.MapPost("/", Create).RequirePermission("admin.permissions.create");
        group.MapPut("/{externalId:guid}", Update).RequirePermission("admin.permissions.update");
        group.MapDelete("/{externalId:guid}", Delete).RequirePermission("admin.permissions.delete");
    }

    private static async Task<IResult> GetAll(
        [FromServices] AuthDbContext db,
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
        [FromServices] AuthDbContext db,
        CancellationToken ct)
    {
        var categories = await db.Permissions
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return Results.Ok(categories);
    }

    private static async Task<IResult> Create(
        [FromBody] CreatePermissionRequest request,
        [FromServices] AuthDbContext db,
        CancellationToken ct)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest(new { message = "O nome é obrigatório." });

        // Validate format: category.resource.action
        var parts = name.Split('.');
        if (parts.Length < 2)
            return Results.BadRequest(new { message = "O nome deve seguir o formato 'categoria.recurso.ação' (ex: admin.users.view)." });

        // Check uniqueness
        var exists = await db.Permissions.AnyAsync(p => p.Name == name, ct);
        if (exists)
            return Results.BadRequest(new { message = $"Já existe uma permissão com o nome '{name}'." });

        var category = parts[0];

        var permission = new PermissionEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = name,
            Category = category,
            Description = request.Description?.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        db.Permissions.Add(permission);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { externalId = permission.ExternalId });
    }

    private static async Task<IResult> Update(
        [FromRoute] Guid externalId,
        [FromBody] UpdatePermissionRequest request,
        [FromServices] AuthDbContext db,
        CancellationToken ct)
    {
        var permission = await db.Permissions.FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);
        if (permission is null)
            return Results.NotFound(new { message = "Permissão não encontrada." });

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest(new { message = "O nome é obrigatório." });

        var parts = name.Split('.');
        if (parts.Length < 2)
            return Results.BadRequest(new { message = "O nome deve seguir o formato 'categoria.recurso.ação' (ex: admin.users.view)." });

        // Check uniqueness (excluding self)
        var exists = await db.Permissions.AnyAsync(p => p.Name == name && p.Id != permission.Id, ct);
        if (exists)
            return Results.BadRequest(new { message = $"Já existe uma permissão com o nome '{name}'." });

        permission.Name = name;
        permission.Category = parts[0];
        permission.Description = request.Description?.Trim();

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Permissão atualizada com sucesso." });
    }

    private static async Task<IResult> Delete(
        [FromRoute] Guid externalId,
        [FromServices] AuthDbContext db,
        CancellationToken ct)
    {
        var permission = await db.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (permission is null)
            return Results.NotFound(new { message = "Permissão não encontrada." });

        if (permission.RolePermissions.Count > 0)
            return Results.BadRequest(new { message = "Não é possível eliminar uma permissão que está atribuída a roles. Remova a permissão das roles primeiro." });

        db.Permissions.Remove(permission);
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Permissão eliminada com sucesso." });
    }
}

public record CreatePermissionRequest(string Name, string? Description);
public record UpdatePermissionRequest(string Name, string? Description);
