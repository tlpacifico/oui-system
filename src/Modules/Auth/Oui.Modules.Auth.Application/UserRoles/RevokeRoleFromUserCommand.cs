using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.UserRoles;

public sealed record RevokeRoleFromUserCommand(
    Guid UserExternalId,
    Guid RoleExternalId) : ICommand;
