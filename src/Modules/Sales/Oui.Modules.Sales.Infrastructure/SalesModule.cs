using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Oui.Modules.Sales.Infrastructure;

public static class SalesModule
{
    public static IServiceCollection AddSalesModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<SalesDbContext>(options =>
            options.UseNpgsql(
                config.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName, Schemas.Sales)));

        return services;
    }
}
