using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure.Abstractions;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class CreateUserCommandHandler(AuthDbContext db, IFirebaseAuthService firebaseAuth)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return Result.Failure<Guid>(UserErrors.EmailAlreadyExists);

        string? firebaseUid = null;
        try
        {
            firebaseUid = await firebaseAuth.CreateUserAsync(
                request.Email, request.Password, request.DisplayName, cancellationToken);
        }
        catch
        {
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
            // Rollback Firebase user if DB save fails
            try { await firebaseAuth.DeleteUserAsync(firebaseUid, cancellationToken); } catch { }
            throw;
        }

        return user.ExternalId;
    }
}
