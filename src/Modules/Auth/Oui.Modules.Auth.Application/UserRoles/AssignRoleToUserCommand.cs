using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.UserRoles;

public sealed record AssignRoleToUserCommand(
    Guid UserExternalId,
    Guid RoleExternalId,
    string AssignedBy) : ICommand;
