using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.RolePermissions;

internal sealed class AssignPermissionToRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<AssignPermissionToRoleCommand>
{
    public async Task<Result> Handle(
        AssignPermissionToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(
            r => r.ExternalId == request.RoleExternalId, cancellationToken);
        if (role is null)
            return Result.Failure(RolePermissionErrors.RoleNotFound);

        var permission = await db.Permissions.FirstOrDefaultAsync(
            p => p.ExternalId == request.PermissionExternalId, cancellationToken);
        if (permission is null)
            return Result.Failure(RolePermissionErrors.PermissionNotFound);

        var exists = await db.RolePermissions
            .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id, cancellationToken);
        if (exists)
            return Result.Failure(RolePermissionErrors.AlreadyAssigned);

        var rolePermission = new RolePermissionEntity
        {
            RoleId = role.Id,
            PermissionId = permission.Id,
            GrantedOn = DateTime.UtcNow,
            GrantedBy = request.GrantedBy
        };

        db.RolePermissions.Add(rolePermission);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
