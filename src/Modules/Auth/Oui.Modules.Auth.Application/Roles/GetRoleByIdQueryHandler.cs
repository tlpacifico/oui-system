using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

internal sealed class GetRoleByIdQueryHandler(AuthDbContext db)
    : IQueryHandler<GetRoleByIdQuery, RoleDetailResponse>
{
    public async Task<Result<RoleDetailResponse>> Handle(
        GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .Where(r => r.ExternalId == request.ExternalId)
            .Select(r => new RoleDetailResponse(
                r.ExternalId,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.UserRoles.Count,
                r.RolePermissions.Count,
                r.CreatedOn,
                r.CreatedBy,
                r.UpdatedOn,
                r.UpdatedBy,
                r.RolePermissions.Select(rp => new RolePermissionSummary(
                    rp.Permission.ExternalId,
                    rp.Permission.Name,
                    rp.Permission.Category,
                    rp.Permission.Description)).ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
            return Result.Failure<RoleDetailResponse>(RoleErrors.NotFound);

        return role;
    }
}
