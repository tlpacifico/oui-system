using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

internal sealed class GetAllRolesQueryHandler(AuthDbContext db)
    : IQueryHandler<GetAllRolesQuery, List<RoleListResponse>>
{
    public async Task<Result<List<RoleListResponse>>> Handle(
        GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Roles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(r =>
                r.Name.Contains(request.Search) ||
                (r.Description != null && r.Description.Contains(request.Search)));

        var roles = await query
            .OrderBy(r => r.Name)
            .Select(r => new RoleListResponse(
                r.ExternalId,
                r.Name,
                r.Description,
                r.IsSystemRole,
                r.UserRoles.Count,
                r.RolePermissions.Count,
                r.CreatedOn,
                r.CreatedBy,
                r.UpdatedOn,
                r.UpdatedBy))
            .ToListAsync(cancellationToken);

        return roles;
    }
}
