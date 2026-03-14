using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.UserRoles;

public static class UserRoleErrors
{
    public static readonly Error UserNotFound = Error.NotFound(
        "UserRole.UserNotFound",
        "User not found.");

    public static readonly Error RoleNotFound = Error.NotFound(
        "UserRole.RoleNotFound",
        "Role not found.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "UserRole.AlreadyAssigned",
        "Role already assigned to user.");

    public static readonly Error NotAssigned = Error.NotFound(
        "UserRole.NotAssigned",
        "Role not assigned to user.");

    public static readonly Error SomeRolesNotFound = Error.Problem(
        "UserRole.SomeRolesNotFound",
        "Some roles not found.");
}
