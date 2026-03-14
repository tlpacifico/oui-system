using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class GetUserByIdQueryHandler(AuthDbContext db)
    : IQueryHandler<GetUserByIdQuery, UserDetailResponse>
{
    public async Task<Result<UserDetailResponse>> Handle(
        GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .Where(u => u.ExternalId == request.ExternalId)
            .Select(u => new UserDetailResponse(
                u.ExternalId,
                u.Email,
                u.DisplayName,
                u.CreatedOn,
                u.UserRoles.Select(ur => new UserRoleDetail(
                    ur.Role.ExternalId,
                    ur.Role.Name,
                    ur.AssignedOn,
                    ur.AssignedBy)).ToList(),
                u.UserRoles.Count))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
            return Result.Failure<UserDetailResponse>(UserErrors.NotFound);

        return user;
    }
}
