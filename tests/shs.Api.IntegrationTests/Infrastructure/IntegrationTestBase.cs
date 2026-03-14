using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Oui.Modules.Auth.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using Oui.Modules.Ecommerce.Infrastructure;
using Oui.Modules.System.Infrastructure;
using Respawn;
using Xunit;

namespace shs.Api.IntegrationTests.Infrastructure;

[Collection("Integration")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected OuiWebApplicationFactory Factory { get; }

    private Respawner? _respawner;
    private readonly string _connectionString;

    protected IntegrationTestBase(PostgresContainerFixture dbFixture)
    {
        Factory = new OuiWebApplicationFactory(dbFixture);
        _connectionString = dbFixture.ConnectionString;
    }

    public async Task InitializeAsync()
    {
        // Ensure the host is started and migrations have run
        _ = Factory.Services;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["auth", "inventory", "sales", "ecommerce", "system"],
            TablesToIgnore =
            [
                new Respawn.Graph.Table("auth", "Roles"),
                new Respawn.Graph.Table("auth", "Permissions"),
                new Respawn.Graph.Table("auth", "RolePermissions"),
                new Respawn.Graph.Table("auth", "Users"),
                new Respawn.Graph.Table("auth", "UserRoles"),
                new Respawn.Graph.Table("system", "SystemSettings"),
                new Respawn.Graph.Table("auth", "__EFMigrationsHistory"),
                new Respawn.Graph.Table("inventory", "__EFMigrationsHistory"),
                new Respawn.Graph.Table("sales", "__EFMigrationsHistory"),
                new Respawn.Graph.Table("ecommerce", "__EFMigrationsHistory"),
                new Respawn.Graph.Table("system", "__EFMigrationsHistory"),
            ]
        });

        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    protected AuthDbContext CreateAuthDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    }

    protected InventoryDbContext CreateInventoryDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    }

    protected SalesDbContext CreateSalesDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SalesDbContext>();
    }

    protected EcommerceDbContext CreateEcommerceDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
    }

    protected SystemDbContext CreateSystemDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<SystemDbContext>();
    }
}
