using Microsoft.EntityFrameworkCore;
using Oui.Modules.System.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;

internal sealed class GetAllSettingsQueryHandler(SystemDbContext db)
    : IQueryHandler<GetAllSettingsQuery, List<SystemSettingGroupResponse>>
{
    public async Task<Result<List<SystemSettingGroupResponse>>> Handle(
        GetAllSettingsQuery query, CancellationToken cancellationToken)
    {
        var settings = await db.SystemSettings
            .OrderBy(s => s.Module)
            .ThenBy(s => s.DisplayName)
            .Select(s => new SystemSettingResponse(
                s.Key, s.Value, s.ValueType, s.Module, s.DisplayName, s.Description))
            .ToListAsync(cancellationToken);

        var grouped = settings
            .GroupBy(s => s.Module)
            .Select(g => new SystemSettingGroupResponse(g.Key, g.ToList()))
            .ToList();

        return grouped;
    }
}
