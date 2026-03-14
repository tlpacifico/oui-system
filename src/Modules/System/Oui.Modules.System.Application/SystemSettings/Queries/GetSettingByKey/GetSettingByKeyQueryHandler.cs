using Microsoft.EntityFrameworkCore;
using Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;
using Oui.Modules.System.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.System.Application.SystemSettings.Queries.GetSettingByKey;

internal sealed class GetSettingByKeyQueryHandler(SystemDbContext db)
    : IQueryHandler<GetSettingByKeyQuery, SystemSettingResponse>
{
    public async Task<Result<SystemSettingResponse>> Handle(
        GetSettingByKeyQuery query, CancellationToken cancellationToken)
    {
        var setting = await db.SystemSettings
            .Where(s => s.Key == query.Key)
            .Select(s => new SystemSettingResponse(
                s.Key, s.Value, s.ValueType, s.Module, s.DisplayName, s.Description))
            .FirstOrDefaultAsync(cancellationToken);

        if (setting is null)
            return Result.Failure<SystemSettingResponse>(SystemSettingErrors.NotFound(query.Key));

        return setting;
    }
}
