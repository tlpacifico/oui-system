namespace Oui.Modules.Auth.Application.UserRoles;

public sealed record UserRoleResponse(
    Guid RoleId,
    string Name,
    string? Description,
    DateTime AssignedOn,
    string? AssignedBy);
