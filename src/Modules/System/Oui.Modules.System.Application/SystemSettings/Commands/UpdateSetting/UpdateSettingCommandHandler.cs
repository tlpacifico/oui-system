using Microsoft.EntityFrameworkCore;
using Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;
using Oui.Modules.System.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.System.Application.SystemSettings.Commands.UpdateSetting;

internal sealed class UpdateSettingCommandHandler(SystemDbContext db)
    : ICommandHandler<UpdateSettingCommand, SystemSettingResponse>
{
    public async Task<Result<SystemSettingResponse>> Handle(
        UpdateSettingCommand command, CancellationToken cancellationToken)
    {
        var setting = await db.SystemSettings
            .FirstOrDefaultAsync(s => s.Key == command.Key, cancellationToken);

        if (setting is null)
            return Result.Failure<SystemSettingResponse>(SystemSettingErrors.NotFound(command.Key));

        setting.Value = command.Value;
        setting.UpdatedBy = command.UserEmail;
        setting.UpdatedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new SystemSettingResponse(
            setting.Key, setting.Value, setting.ValueType,
            setting.Module, setting.DisplayName, setting.Description);
    }
}
