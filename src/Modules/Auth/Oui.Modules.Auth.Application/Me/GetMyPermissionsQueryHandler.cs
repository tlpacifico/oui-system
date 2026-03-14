using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Me;

internal sealed class GetMyPermissionsQueryHandler(AuthDbContext db)
    : IQueryHandler<GetMyPermissionsQuery, List<MyPermissionResponse>>
{
    public async Task<Result<List<MyPermissionResponse>>> Handle(
        GetMyPermissionsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Email))
            return Result.Failure<List<MyPermissionResponse>>(MeErrors.Unauthorized);

        var permissions = await db.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.User.Email == request.Email)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission))
            .Distinct()
            .Select(p => new MyPermissionResponse(p.Name))
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
