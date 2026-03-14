using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class DeletePermissionCommandHandler(AuthDbContext db)
    : ICommandHandler<DeletePermissionCommand>
{
    public async Task<Result> Handle(
        DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await db.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (permission is null)
            return Result.Failure(PermissionErrors.NotFound);

        if (permission.RolePermissions.Count > 0)
            return Result.Failure(PermissionErrors.HasAssignedRoles);

        db.Permissions.Remove(permission);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
