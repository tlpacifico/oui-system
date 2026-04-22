using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure.Services;

namespace Oui.Modules.Auth.Infrastructure;

public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AuthDbContext>((sp, options) =>
            options.UseNpgsql(
                    config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsHistoryTable(
                        HistoryRepository.DefaultTableName, Schemas.Auth))
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>()));

        InitializeFirebase(config);
        services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

        return services;
    }

    private static void InitializeFirebase(IConfiguration config)
    {
        if (FirebaseApp.DefaultInstance != null)
            return;

        try
        {
            var serviceAccountPath = config["Firebase:ServiceAccountPath"];
            if (!string.IsNullOrEmpty(serviceAccountPath) && File.Exists(serviceAccountPath))
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath)
                });
            }
            else
            {
                // Use Application Default Credentials (e.g., GOOGLE_APPLICATION_CREDENTIALS env var)
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.GetApplicationDefault()
                });
            }
        }
        catch
        {
            // Firebase Admin SDK initialization may fail at design-time (e.g., EF migrations)
            // or in test environments. The service will throw at runtime if used without initialization.
        }
    }
}
