using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.UserRoles;

internal sealed class AssignRoleToUserCommandHandler(AuthDbContext db)
    : ICommandHandler<AssignRoleToUserCommand>
{
    public async Task<Result> Handle(
        AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.ExternalId == request.UserExternalId, cancellationToken);
        if (user is null)
            return Result.Failure(UserRoleErrors.UserNotFound);

        var role = await db.Roles.FirstOrDefaultAsync(
            r => r.ExternalId == request.RoleExternalId, cancellationToken);
        if (role is null)
            return Result.Failure(UserRoleErrors.RoleNotFound);

        var exists = await db.UserRoles
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);
        if (exists)
            return Result.Failure(UserRoleErrors.AlreadyAssigned);

        var userRole = new UserRoleEntity
        {
            UserId = user.Id,
            RoleId = role.Id,
            AssignedOn = DateTime.UtcNow,
            AssignedBy = request.AssignedBy
        };

        db.UserRoles.Add(userRole);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
