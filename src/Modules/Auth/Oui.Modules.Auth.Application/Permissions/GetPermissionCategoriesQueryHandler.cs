using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class GetPermissionCategoriesQueryHandler(AuthDbContext db)
    : IQueryHandler<GetPermissionCategoriesQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(
        GetPermissionCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.Permissions
            .Select(p => p.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        return categories;
    }
}
