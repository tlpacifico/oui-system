using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.RolePermissions;

internal sealed class AssignBulkPermissionsToRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<AssignBulkPermissionsToRoleCommand, int>
{
    public async Task<Result<int>> Handle(
        AssignBulkPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.ExternalId == request.RoleExternalId, cancellationToken);

        if (role is null)
            return Result.Failure<int>(RolePermissionErrors.RoleNotFound);

        var permissions = await db.Permissions
            .Where(p => request.PermissionExternalIds.Contains(p.ExternalId))
            .ToListAsync(cancellationToken);

        if (permissions.Count != request.PermissionExternalIds.Count)
            return Result.Failure<int>(RolePermissionErrors.SomePermissionsNotFound);

        var existingPermissionIds = role.RolePermissions.Select(rp => rp.PermissionId).ToHashSet();
        var newRolePermissions = permissions
            .Where(p => !existingPermissionIds.Contains(p.Id))
            .Select(p => new RolePermissionEntity
            {
                RoleId = role.Id,
                PermissionId = p.Id,
                GrantedOn = DateTime.UtcNow,
                GrantedBy = request.GrantedBy
            })
            .ToList();

        if (newRolePermissions.Any())
        {
            db.RolePermissions.AddRange(newRolePermissions);
            await db.SaveChangesAsync(cancellationToken);
        }

        return newRolePermissions.Count;
    }
}
