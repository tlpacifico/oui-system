using shs.Domain.Results;

namespace Oui.Modules.System.Application.SystemSettings;

public static class SystemSettingErrors
{
    public static Error NotFound(string key) => Error.NotFound(
        "SystemSetting.NotFound", $"Setting '{key}' not found");
}
