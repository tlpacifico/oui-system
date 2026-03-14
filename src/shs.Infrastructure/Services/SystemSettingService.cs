using Microsoft.EntityFrameworkCore;
using Oui.Modules.System.Infrastructure;

namespace shs.Infrastructure.Services;

public class SystemSettingService
{
    private readonly SystemDbContext _db;

    public SystemSettingService(SystemDbContext db)
    {
        _db = db;
    }

    public async Task<bool> GetBool(string key, bool defaultValue = false)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return defaultValue;
        return bool.TryParse(setting.Value, out var result) ? result : defaultValue;
    }

    public async Task<decimal> GetDecimal(string key, decimal defaultValue = 0)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        if (setting == null) return defaultValue;
        return decimal.TryParse(setting.Value, System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : defaultValue;
    }

    public async Task<string?> GetString(string key)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        return setting?.Value;
    }
}
