using FirebaseAdmin.Auth;
using Oui.Modules.Auth.Infrastructure.Abstractions;

namespace Oui.Modules.Auth.Infrastructure.Services;

internal sealed class FirebaseAuthService : IFirebaseAuthService
{
    public async Task<string> CreateUserAsync(string email, string password, string? displayName, CancellationToken ct = default)
    {
        var args = new UserRecordArgs
        {
            Email = email,
            Password = password,
            DisplayName = displayName,
            EmailVerified = false
        };

        var userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(args, ct);
        return userRecord.Uid;
    }

    public async Task UpdateUserAsync(string uid, string? displayName, CancellationToken ct = default)
    {
        var args = new UserRecordArgs
        {
            Uid = uid,
            DisplayName = displayName
        };

        await FirebaseAuth.DefaultInstance.UpdateUserAsync(args, ct);
    }

    public async Task DeleteUserAsync(string uid, CancellationToken ct = default)
    {
        await FirebaseAuth.DefaultInstance.DeleteUserAsync(uid, ct);
    }
}
