using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.RolePermissions;

public sealed record AssignBulkPermissionsToRoleCommand(
    Guid RoleExternalId,
    List<Guid> PermissionExternalIds,
    string GrantedBy) : ICommand<int>;
