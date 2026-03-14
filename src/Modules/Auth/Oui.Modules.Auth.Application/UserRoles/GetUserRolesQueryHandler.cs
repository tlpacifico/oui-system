using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.UserRoles;

internal sealed class GetUserRolesQueryHandler(AuthDbContext db)
    : IQueryHandler<GetUserRolesQuery, List<UserRoleResponse>>
{
    public async Task<Result<List<UserRoleResponse>>> Handle(
        GetUserRolesQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.ExternalId == request.UserExternalId, cancellationToken);

        if (user is null)
            return Result.Failure<List<UserRoleResponse>>(UserRoleErrors.UserNotFound);

        var roles = await db.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Include(ur => ur.Role)
            .Select(ur => new UserRoleResponse(
                ur.Role.ExternalId,
                ur.Role.Name,
                ur.Role.Description,
                ur.AssignedOn,
                ur.AssignedBy))
            .ToListAsync(cancellationToken);

        return roles;
    }
}
