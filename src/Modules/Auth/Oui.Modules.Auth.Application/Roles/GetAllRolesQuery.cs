using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Roles;

public sealed record GetAllRolesQuery(string? Search) : IQuery<List<RoleListResponse>>;
