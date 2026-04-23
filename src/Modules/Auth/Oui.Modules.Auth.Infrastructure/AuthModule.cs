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
            var credential = ResolveCredential(config)
                ?? throw new InvalidOperationException(
                    "No Firebase credentials found. Set Firebase:ServiceAccountJson (JSON content), " +
                    "Firebase:ServiceAccountPath (file path), or GOOGLE_APPLICATION_CREDENTIALS.");

            FirebaseApp.Create(new AppOptions { Credential = credential });
        }
        catch (Exception ex)
        {
            // Tolerated so EF design-time and tests can boot without Firebase credentials,
            // but surface the cause so runtime misconfiguration is diagnosable.
            Console.Error.WriteLine($"[AuthModule] Firebase Admin SDK init skipped: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static GoogleCredential? ResolveCredential(IConfiguration config)
    {
        var json = config["Firebase:ServiceAccountJson"];
        if (!string.IsNullOrWhiteSpace(json))
            return GoogleCredential.FromJson(json);

        var path = config["Firebase:ServiceAccountPath"];
        if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            return GoogleCredential.FromFile(path);

        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS")))
            return GoogleCredential.GetApplicationDefault();

        return null;
    }
}
