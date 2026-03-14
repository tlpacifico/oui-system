using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.UserRoles;

internal sealed class AssignBulkRolesToUserCommandHandler(AuthDbContext db)
    : ICommandHandler<AssignBulkRolesToUserCommand, int>
{
    public async Task<Result<int>> Handle(
        AssignBulkRolesToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.ExternalId == request.UserExternalId, cancellationToken);

        if (user is null)
            return Result.Failure<int>(UserRoleErrors.UserNotFound);

        var roles = await db.Roles
            .Where(r => request.RoleExternalIds.Contains(r.ExternalId))
            .ToListAsync(cancellationToken);

        if (roles.Count != request.RoleExternalIds.Count)
            return Result.Failure<int>(UserRoleErrors.SomeRolesNotFound);

        var existingRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet();
        var newUserRoles = roles
            .Where(r => !existingRoleIds.Contains(r.Id))
            .Select(r => new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = r.Id,
                AssignedOn = DateTime.UtcNow,
                AssignedBy = request.AssignedBy
            })
            .ToList();

        if (newUserRoles.Any())
        {
            db.UserRoles.AddRange(newUserRoles);
            await db.SaveChangesAsync(cancellationToken);
        }

        return newUserRoles.Count;
    }
}
