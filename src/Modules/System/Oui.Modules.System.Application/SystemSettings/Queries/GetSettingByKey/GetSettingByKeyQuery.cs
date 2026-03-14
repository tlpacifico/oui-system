using Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;
using shs.Application.Messaging;

namespace Oui.Modules.System.Application.SystemSettings.Queries.GetSettingByKey;

public sealed record GetSettingByKeyQuery(string Key) : IQuery<SystemSettingResponse>;
