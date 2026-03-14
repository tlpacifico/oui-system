using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.UserRoles;

public sealed record AssignBulkRolesToUserCommand(
    Guid UserExternalId,
    List<Guid> RoleExternalIds,
    string AssignedBy) : ICommand<int>;
