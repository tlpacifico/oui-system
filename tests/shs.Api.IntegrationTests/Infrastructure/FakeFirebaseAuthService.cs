using Oui.Modules.Auth.Infrastructure.Abstractions;

namespace shs.Api.IntegrationTests.Infrastructure;

public class FakeFirebaseAuthService : IFirebaseAuthService
{
    public Task<string> CreateUserAsync(string email, string password, string? displayName, CancellationToken ct = default)
    {
        return Task.FromResult($"test-firebase-uid-{Guid.NewGuid()}");
    }

    public Task UpdateUserAsync(string uid, string? displayName, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteUserAsync(string uid, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }
}
