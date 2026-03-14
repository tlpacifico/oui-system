using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Oui.Modules.Ecommerce.Infrastructure;

public static class EcommerceModule
{
    public static IServiceCollection AddEcommerceModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<EcommerceDbContext>((sp, options) =>
            options.UseNpgsql(
                    config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName, Schemas.Ecommerce))
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>()));

        return services;
    }
}
