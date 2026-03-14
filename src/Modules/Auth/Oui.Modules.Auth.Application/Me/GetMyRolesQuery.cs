using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Me;

public sealed record GetMyRolesQuery(string Email) : IQuery<List<MyRoleResponse>>;
