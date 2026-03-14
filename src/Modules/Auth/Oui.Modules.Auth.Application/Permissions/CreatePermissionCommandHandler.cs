using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class CreatePermissionCommandHandler(AuthDbContext db)
    : ICommandHandler<CreatePermissionCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreatePermissionCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Guid>(PermissionErrors.NameRequired);

        var parts = name.Split('.');
        if (parts.Length < 2)
            return Result.Failure<Guid>(PermissionErrors.InvalidNameFormat);

        var exists = await db.Permissions.AnyAsync(p => p.Name == name, cancellationToken);
        if (exists)
            return Result.Failure<Guid>(PermissionErrors.NameAlreadyExists);

        var permission = new PermissionEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = name,
            Category = parts[0],
            Description = request.Description?.Trim(),
            CreatedOn = DateTime.UtcNow
        };

        db.Permissions.Add(permission);
        await db.SaveChangesAsync(cancellationToken);

        return permission.ExternalId;
    }
}
