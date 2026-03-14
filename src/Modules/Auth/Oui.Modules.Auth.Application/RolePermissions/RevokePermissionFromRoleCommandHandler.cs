using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.RolePermissions;

internal sealed class RevokePermissionFromRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<RevokePermissionFromRoleCommand>
{
    public async Task<Result> Handle(
        RevokePermissionFromRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(
            r => r.ExternalId == request.RoleExternalId, cancellationToken);
        if (role is null)
            return Result.Failure(RolePermissionErrors.RoleNotFound);

        var permission = await db.Permissions.FirstOrDefaultAsync(
            p => p.ExternalId == request.PermissionExternalId, cancellationToken);
        if (permission is null)
            return Result.Failure(RolePermissionErrors.PermissionNotFound);

        var rolePermission = await db.RolePermissions
            .FirstOrDefaultAsync(
                rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, cancellationToken);

        if (rolePermission is null)
            return Result.Failure(RolePermissionErrors.NotAssigned);

        db.RolePermissions.Remove(rolePermission);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
