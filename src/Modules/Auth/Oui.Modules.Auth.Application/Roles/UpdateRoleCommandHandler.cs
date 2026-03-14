using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

internal sealed class UpdateRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<UpdateRoleCommand>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await db.Roles.FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);
        if (role is null)
            return Result.Failure(RoleErrors.NotFound);

        if (role.IsSystemRole)
            return Result.Failure(RoleErrors.CannotModifySystemRole);

        if (request.Name != role.Name &&
            await db.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
            return Result.Failure(RoleErrors.NameAlreadyExists);

        role.Name = request.Name;
        role.Description = request.Description;
        role.UpdatedOn = DateTime.UtcNow;
        role.UpdatedBy = request.UpdatedBy;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
