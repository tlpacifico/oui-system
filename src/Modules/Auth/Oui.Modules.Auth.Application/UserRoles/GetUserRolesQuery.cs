using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.UserRoles;

public sealed record GetUserRolesQuery(Guid UserExternalId) : IQuery<List<UserRoleResponse>>;
