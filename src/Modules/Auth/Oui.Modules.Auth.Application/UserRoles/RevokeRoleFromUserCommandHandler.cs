using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.UserRoles;

internal sealed class RevokeRoleFromUserCommandHandler(AuthDbContext db)
    : ICommandHandler<RevokeRoleFromUserCommand>
{
    public async Task<Result> Handle(
        RevokeRoleFromUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.ExternalId == request.UserExternalId, cancellationToken);
        if (user is null)
            return Result.Failure(UserRoleErrors.UserNotFound);

        var role = await db.Roles.FirstOrDefaultAsync(
            r => r.ExternalId == request.RoleExternalId, cancellationToken);
        if (role is null)
            return Result.Failure(UserRoleErrors.RoleNotFound);

        var userRole = await db.UserRoles
            .FirstOrDefaultAsync(
                ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);

        if (userRole is null)
            return Result.Failure(UserRoleErrors.NotAssigned);

        db.UserRoles.Remove(userRole);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
