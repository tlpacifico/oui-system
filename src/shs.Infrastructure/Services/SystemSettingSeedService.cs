using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using Oui.Modules.System.Infrastructure;

namespace shs.Infrastructure.Services;

public class SystemSettingSeedService
{
    private readonly SystemDbContext _db;

    public SystemSettingSeedService(SystemDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var defaults = new List<(string Key, string Value, string ValueType, string Module, string DisplayName, string? Description)>
        {
            ("email.enabled", "true", "bool", "general", "Habilitar envio de emails", "Controla se o sistema envia emails (notificações, recibos, etc.)"),
            ("consignment.enabled", "true", "bool", "consignment", "Habilitar módulo de consignação", "Ativa ou desativa o módulo de consignação no sistema"),
            ("consignment.default_commission_percentage", "30", "decimal", "consignment", "Percentual de comissão padrão", "Percentual de comissão aplicado por padrão em novas consignações"),
            ("pos.auto_create_settlement", "false", "bool", "pos", "Criação automática de settlement após venda", "Quando ativo, cria automaticamente um acerto após cada venda"),
            ("audit.retention_days", "365", "integer", "system", "Audit log retention (days)", "Number of days to retain audit log records before cleanup"),
        };

        foreach (var (key, value, valueType, module, displayName, description) in defaults)
        {
            var exists = await _db.SystemSettings.AnyAsync(s => s.Key == key);
            if (!exists)
            {
                _db.SystemSettings.Add(new SystemSettingEntity
                {
                    ExternalId = Guid.NewGuid(),
                    Key = key,
                    Value = value,
                    ValueType = valueType,
                    Module = module,
                    DisplayName = displayName,
                    Description = description,
                    CreatedOn = now,
                    CreatedBy = "system"
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
