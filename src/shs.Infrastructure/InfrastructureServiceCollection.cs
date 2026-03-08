using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using shs.Domain.Notifications;
using shs.Infrastructure.Database;
using shs.Infrastructure.Notifications;
using shs.Infrastructure.Services;
using shs.Infrastructure.Services.Import;

namespace shs.Infrastructure;

public static class InfrastructureServiceCollection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string not found");

        services.AddDbContext<ShsDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register services
        services.AddScoped<IItemIdGeneratorService, ItemIdGeneratorService>();

        // Email service
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, EmailService>();

        // System settings
        services.AddScoped<SystemSettingService>();

        // Notification infrastructure
        services.AddScoped<ISaleNotificationHandler, AutoSettlementHandler>();
        services.AddScoped<ISaleNotificationDispatcher, SaleNotificationDispatcher>();

        // Import services
        services.AddScoped<ExcelEstoqueReader>();
        services.AddScoped<ExcelConsignadosReader>();
        services.AddScoped<ImportService>();

        // Ecommerce background services
        services.AddHostedService<EcommerceReservationExpirationService>();

        return services;
    }
}
