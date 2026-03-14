using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.RolePermissions;

public static class RolePermissionErrors
{
    public static readonly Error RoleNotFound = Error.NotFound(
        "RolePermission.RoleNotFound",
        "Role not found.");

    public static readonly Error PermissionNotFound = Error.NotFound(
        "RolePermission.PermissionNotFound",
        "Permission not found.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "RolePermission.AlreadyAssigned",
        "Permission already assigned to role.");

    public static readonly Error NotAssigned = Error.NotFound(
        "RolePermission.NotAssigned",
        "Permission not assigned to role.");

    public static readonly Error SomePermissionsNotFound = Error.Problem(
        "RolePermission.SomePermissionsNotFound",
        "Some permissions not found.");
}
