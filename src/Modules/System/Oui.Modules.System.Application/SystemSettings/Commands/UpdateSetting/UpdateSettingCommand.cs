using Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;
using shs.Application.Messaging;

namespace Oui.Modules.System.Application.SystemSettings.Commands.UpdateSetting;

public sealed record UpdateSettingCommand(string Key, string Value, string? UserEmail) : ICommand<SystemSettingResponse>;
