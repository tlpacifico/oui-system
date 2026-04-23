using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Oui.Modules.Auth.Infrastructure.Abstractions;

namespace Oui.Modules.Auth.Infrastructure.Services;

internal sealed class FirebaseAuthService : IFirebaseAuthService
{
    private static FirebaseAuth Auth =>
        FirebaseApp.DefaultInstance is null
            ? throw new InvalidOperationException(
                "Firebase Admin SDK is not initialized. Configure 'Firebase:ServiceAccountPath' " +
                "in user secrets/appsettings, or set the GOOGLE_APPLICATION_CREDENTIALS environment " +
                "variable to a service account JSON key file.")
            : FirebaseAuth.DefaultInstance;

    public async Task<string> CreateUserAsync(string email, string password, string? displayName, CancellationToken ct = default)
    {
        var args = new UserRecordArgs
        {
            Email = email,
            Password = password,
            DisplayName = displayName,
            EmailVerified = false
        };

        var userRecord = await Auth.CreateUserAsync(args, ct);
        return userRecord.Uid;
    }

    public async Task UpdateUserAsync(string uid, string? displayName, CancellationToken ct = default)
    {
        var args = new UserRecordArgs
        {
            Uid = uid,
            DisplayName = displayName
        };

        await Auth.UpdateUserAsync(args, ct);
    }

    public async Task DeleteUserAsync(string uid, CancellationToken ct = default)
    {
        await Auth.DeleteUserAsync(uid, ct);
    }
}
