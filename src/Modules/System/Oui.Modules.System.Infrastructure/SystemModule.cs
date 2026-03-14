using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Oui.Modules.System.Infrastructure;

public static class SystemModule
{
    public static IServiceCollection AddSystemModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SystemDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName, Schemas.System)));

        // Note: AuditInterceptor is NOT added to SystemDbContext to avoid circular tracking
        // (the interceptor writes audit logs TO SystemDbContext)

        return services;
    }
}
