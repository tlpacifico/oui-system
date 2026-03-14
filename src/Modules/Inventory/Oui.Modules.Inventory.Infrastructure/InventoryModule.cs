using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Oui.Modules.Inventory.Infrastructure;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<InventoryDbContext>((sp, options) =>
            options.UseNpgsql(
                    config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName, Schemas.Inventory))
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>()));

        return services;
    }
}
