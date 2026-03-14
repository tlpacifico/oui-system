namespace Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;

public sealed record SystemSettingResponse(
    string Key,
    string Value,
    string ValueType,
    string Module,
    string DisplayName,
    string? Description);

public sealed record SystemSettingGroupResponse(
    string Module,
    List<SystemSettingResponse> Settings);
