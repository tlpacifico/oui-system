using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Me;

public sealed record GetMyPermissionsQuery(string Email) : IQuery<List<MyPermissionResponse>>;
