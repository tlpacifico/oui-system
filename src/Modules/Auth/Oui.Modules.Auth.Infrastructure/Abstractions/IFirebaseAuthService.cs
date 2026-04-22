namespace Oui.Modules.Auth.Infrastructure.Abstractions;

public interface IFirebaseAuthService
{
    Task<string> CreateUserAsync(string email, string password, string? displayName, CancellationToken ct = default);
    Task UpdateUserAsync(string uid, string? displayName, CancellationToken ct = default);
    Task DeleteUserAsync(string uid, CancellationToken ct = default);
}
