using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using Oui.Modules.System.Infrastructure;

namespace shs.Api.Admin;

public static class SystemSettingEndpoints
{
    public static void MapSystemSettingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/system-settings").WithTags("System Settings");

        group.MapGet("/", GetAll)
            .RequirePermission("admin.settings.view");

        group.MapGet("/{key}", GetByKey)
            .RequirePermission("admin.settings.view");

        group.MapPut("/{key}", Update)
            .RequirePermission("admin.settings.update");
    }

    private static async Task<IResult> GetAll(SystemDbContext db, CancellationToken ct)
    {
        var settings = await db.SystemSettings
            .OrderBy(s => s.Module)
            .ThenBy(s => s.DisplayName)
            .Select(s => new SystemSettingResponse(s.Key, s.Value, s.ValueType, s.Module, s.DisplayName, s.Description))
            .ToListAsync(ct);

        var grouped = settings
            .GroupBy(s => s.Module)
            .Select(g => new SystemSettingGroupResponse(g.Key, g.ToList()))
            .ToList();

        return Results.Ok(grouped);
    }

    private static async Task<IResult> GetByKey(SystemDbContext db, string key, CancellationToken ct)
    {
        var setting = await db.SystemSettings
            .Where(s => s.Key == key)
            .Select(s => new SystemSettingResponse(s.Key, s.Value, s.ValueType, s.Module, s.DisplayName, s.Description))
            .FirstOrDefaultAsync(ct);

        if (setting == null)
            return Results.NotFound(new { error = $"Setting '{key}' not found" });

        return Results.Ok(setting);
    }

    private static async Task<IResult> Update(
        SystemDbContext db,
        string key,
        UpdateSystemSettingRequest request,
        HttpContext httpContext,
        CancellationToken ct)
    {
        var setting = await db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);

        if (setting == null)
            return Results.NotFound(new { error = $"Setting '{key}' not found" });

        setting.Value = request.Value;
        setting.UpdatedBy = httpContext.User.GetUserEmail();
        setting.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new SystemSettingResponse(
            setting.Key, setting.Value, setting.ValueType, setting.Module, setting.DisplayName, setting.Description));
    }
}

public record SystemSettingResponse(string Key, string Value, string ValueType, string Module, string DisplayName, string? Description);
public record UpdateSystemSettingRequest(string Value);
public record SystemSettingGroupResponse(string Module, List<SystemSettingResponse> Settings);
