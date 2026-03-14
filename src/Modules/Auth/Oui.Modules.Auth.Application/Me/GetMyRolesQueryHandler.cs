using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Me;

internal sealed class GetMyRolesQueryHandler(AuthDbContext db)
    : IQueryHandler<GetMyRolesQuery, List<MyRoleResponse>>
{
    public async Task<Result<List<MyRoleResponse>>> Handle(
        GetMyRolesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email))
            return Result.Failure<List<MyRoleResponse>>(MeErrors.Unauthorized);

        var roles = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.User.Email == request.Email)
            .Include(ur => ur.Role)
            .Select(ur => new MyRoleResponse(ur.Role.Name))
            .ToListAsync(cancellationToken);

        return roles;
    }
}
