using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Roles;

public static class RoleErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Role.NotFound",
        "Role not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Role.NameAlreadyExists",
        "Role name already exists.");

    public static readonly Error CannotModifySystemRole = Error.Problem(
        "Role.CannotModifySystemRole",
        "Cannot modify system roles.");

    public static readonly Error CannotDeleteSystemRole = Error.Problem(
        "Role.CannotDeleteSystemRole",
        "Cannot delete system roles.");

    public static readonly Error CannotDeleteWithAssignedUsers = Error.Problem(
        "Role.CannotDeleteWithAssignedUsers",
        "Cannot delete role with assigned users.");
}
