using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using shs.Domain.Notifications;
using shs.Infrastructure.Interceptors;
using shs.Infrastructure.Notifications;
using shs.Infrastructure.Services;
using shs.Infrastructure.Services.Import;

namespace shs.Infrastructure;

public static class InfrastructureServiceCollection
{
    /// <summary>
    /// Registers shared infrastructure services (email, notifications, import).
    /// Module-specific DbContexts and services are registered by their respective modules.
    /// </summary>
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Audit interceptor (shared across all module DbContexts)
        services.AddScoped<AuditInterceptor>();

        // Email service
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailService, EmailService>();

        // Notification infrastructure
        services.AddScoped<ISaleNotificationHandler, AutoSettlementHandler>();
        services.AddScoped<ISaleNotificationDispatcher, SaleNotificationDispatcher>();

        // Module-specific services (remain in shs.Infrastructure as they depend on module DbContexts)
        services.AddScoped<IItemIdGeneratorService, ItemIdGeneratorService>();
        services.AddScoped<SystemSettingService>();
        services.AddHostedService<EcommerceReservationExpirationService>();

        // Import services
        services.AddScoped<ExcelEstoqueReader>();
        services.AddScoped<ExcelConsignadosReader>();
        services.AddScoped<ImportService>();

        return services;
    }
}
