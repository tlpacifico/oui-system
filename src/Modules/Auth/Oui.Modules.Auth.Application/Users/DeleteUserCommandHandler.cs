using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class DeleteUserCommandHandler(
    AuthDbContext db,
    IFirebaseAuthService firebaseAuth,
    ILogger<DeleteUserCommandHandler> logger)
    : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.ExternalId == request.ExternalId, cancellationToken);

        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (string.Equals(user.Email, request.RequestedBy, StringComparison.OrdinalIgnoreCase))
            return Result.Failure(UserErrors.CannotDeleteSelf);

        db.UserRoles.RemoveRange(user.UserRoles);

        if (!string.IsNullOrEmpty(user.FirebaseUid))
        {
            try
            {
                await firebaseAuth.DeleteUserAsync(user.FirebaseUid, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Firebase DeleteUser failed for uid {FirebaseUid}", user.FirebaseUid);
                return Result.Failure(UserErrors.FirebaseError);
            }
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
