using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class DeleteUserCommandHandler(AuthDbContext db, IFirebaseAuthService firebaseAuth)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.ExternalId == request.ExternalId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        // Prevent self-deletion
        if (string.Equals(user.Email, request.RequestedBy, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(UserErrors.CannotDeleteSelf);

        // Remove user roles
        db.UserRoles.RemoveRange(user.UserRoles);

        // Delete from Firebase
        if (!string.IsNullOrEmpty(user.FirebaseUid))
        {
            try
            {
                await firebaseAuth.DeleteUserAsync(user.FirebaseUid, cancellationToken);
            }
            catch
            {
                return Result.Failure(UserErrors.FirebaseError);
            }
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
