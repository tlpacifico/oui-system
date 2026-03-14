using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

internal sealed class DeleteRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<DeleteRoleCommand>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (role is null)
            return Result.Failure(RoleErrors.NotFound);

        if (role.IsSystemRole)
            return Result.Failure(RoleErrors.CannotDeleteSystemRole);

        if (role.UserRoles.Any())
            return Result.Failure(RoleErrors.CannotDeleteWithAssignedUsers);

        role.IsDeleted = true;
        role.DeletedOn = DateTime.UtcNow;
        role.DeletedBy = request.DeletedBy;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
