using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using shs.Infrastructure.Database;
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
            TablesToIgnore =
            [
                new Respawn.Graph.Table("Roles"),
                new Respawn.Graph.Table("Permissions"),
                new Respawn.Graph.Table("RolePermissions"),
                new Respawn.Graph.Table("Users"),
                new Respawn.Graph.Table("UserRoles"),
                new Respawn.Graph.Table("SystemSettings"),
                new Respawn.Graph.Table("__EFMigrationsHistory"),
            ]
        });

        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }

    protected ShsDbContext CreateDbContext()
    {
        var scope = Factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<ShsDbContext>();
    }
}
