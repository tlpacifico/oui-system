using shs.Application.Messaging;

namespace Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;

public sealed record GetAllSettingsQuery : IQuery<List<SystemSettingGroupResponse>>;
