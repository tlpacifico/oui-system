using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class GetAllUsersQueryHandler(AuthDbContext db)
    : IQueryHandler<GetAllUsersQuery, List<UserListResponse>>
{
    public async Task<Result<List<UserListResponse>>> Handle(
        GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(u =>
                u.Email.Contains(request.Search) ||
                (u.DisplayName != null && u.DisplayName.Contains(request.Search)));

        var users = await query
            .OrderBy(u => u.Email)
            .Select(u => new UserListResponse(
                u.ExternalId,
                u.Email,
                u.DisplayName,
                u.CreatedOn,
                u.UserRoles.Select(ur => new UserRoleSummary(
                    ur.Role.ExternalId,
                    ur.Role.Name)).ToList(),
                u.UserRoles.Count))
            .ToListAsync(cancellationToken);

        return users;
    }
}
