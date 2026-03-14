using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.RolePermissions;

public sealed record RevokePermissionFromRoleCommand(
    Guid RoleExternalId,
    Guid PermissionExternalId) : ICommand;
