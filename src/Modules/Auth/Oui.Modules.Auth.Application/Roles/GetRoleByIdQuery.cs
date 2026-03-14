using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Roles;

public sealed record GetRoleByIdQuery(Guid ExternalId) : IQuery<RoleDetailResponse>;
