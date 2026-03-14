using Microsoft.EntityFrameworkCore;
using Oui.Modules.Auth.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class UpdatePermissionCommandHandler(AuthDbContext db)
    : ICommandHandler<UpdatePermissionCommand>
{
    public async Task<Result> Handle(
        UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await db.Permissions
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (permission is null)
            return Result.Failure(PermissionErrors.NotFound);

        var name = request.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(PermissionErrors.NameRequired);

        var parts = name.Split('.');
        if (parts.Length < 2)
            return Result.Failure(PermissionErrors.InvalidNameFormat);

        var exists = await db.Permissions
            .AnyAsync(p => p.Name == name && p.Id != permission.Id, cancellationToken);
        if (exists)
            return Result.Failure(PermissionErrors.NameAlreadyExists);

        permission.Name = name;
        permission.Category = parts[0];
        permission.Description = request.Description?.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
