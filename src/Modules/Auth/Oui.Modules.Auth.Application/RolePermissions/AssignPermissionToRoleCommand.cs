using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.RolePermissions;

public sealed record AssignPermissionToRoleCommand(
    Guid RoleExternalId,
    Guid PermissionExternalId,
    string GrantedBy) : ICommand;
