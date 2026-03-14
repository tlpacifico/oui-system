using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

internal sealed class CreateRoleCommandHandler(AuthDbContext db)
    : ICommandHandler<CreateRoleCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await db.Roles.AnyAsync(r => r.Name == request.Name, cancellationToken))
            return Result.Failure<Guid>(RoleErrors.NameAlreadyExists);

        var role = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        db.Roles.Add(role);
        await db.SaveChangesAsync(cancellationToken);

        return role.ExternalId;
    }
}
