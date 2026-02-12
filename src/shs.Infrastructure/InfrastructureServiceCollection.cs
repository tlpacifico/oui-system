using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using shs.Infrastructure.Database;

namespace shs.Infrastructure;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Database=oui_system;Username=postgres;Password=postgres";

        services.AddDbContext<ShsDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
