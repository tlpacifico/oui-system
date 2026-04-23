using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class UpdateUserCommandHandler(
    AuthDbContext db,
    IFirebaseAuthService firebaseAuth,
    ILogger<UpdateUserCommandHandler> logger)
    : ICommandHandler<UpdateUserCommand>
{
    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.ExternalId == request.ExternalId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.NotFound);

        if (!string.IsNullOrEmpty(user.FirebaseUid))
        {
            try
            {
                await firebaseAuth.UpdateUserAsync(user.FirebaseUid, request.DisplayName, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Firebase UpdateUser failed for uid {FirebaseUid}", user.FirebaseUid);
                return Result.Failure(UserErrors.FirebaseError);
            }
        }

        user.DisplayName = request.DisplayName;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
