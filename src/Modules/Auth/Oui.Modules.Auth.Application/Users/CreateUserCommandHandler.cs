using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class CreateUserCommandHandler(
    AuthDbContext db,
    IFirebaseAuthService firebaseAuth,
    ILogger<CreateUserCommandHandler> logger)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);

        string? firebaseUid;
        try
        {
            firebaseUid = await firebaseAuth.CreateUserAsync(
                request.Email, request.Password, request.DisplayName, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Firebase CreateUser failed for email {Email}", request.Email);
            return Result.Failure<Guid>(UserErrors.FirebaseError);
        }

        var user = new UserEntity
        {
            ExternalId = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            FirebaseUid = firebaseUid,
            CreatedOn = DateTime.UtcNow
        };

        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try { await firebaseAuth.DeleteUserAsync(firebaseUid, cancellationToken); }
            catch (Exception rollbackEx)
            {
                logger.LogError(rollbackEx, "Failed to rollback Firebase user {FirebaseUid} after DB save error", firebaseUid);
            }
            throw;
        }

        return user.ExternalId;
    }
}
