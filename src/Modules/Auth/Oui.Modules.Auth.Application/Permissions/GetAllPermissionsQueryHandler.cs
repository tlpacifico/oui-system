using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class GetAllPermissionsQueryHandler(AuthDbContext db)
    : IQueryHandler<GetAllPermissionsQuery, List<PermissionResponse>>
{
    public async Task<Result<List<PermissionResponse>>> Handle(
        GetAllPermissionsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Permissions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p =>
                p.Name.Contains(request.Search) ||
                (p.Description != null && p.Description.Contains(request.Search)));

        var permissions = await query
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionResponse(
                p.ExternalId,
                p.Name,
                p.Category,
                p.Description))
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
