using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Permissions;

public sealed record GetAllPermissionsQuery(string? Category, string? Search) : IQuery<List<PermissionResponse>>;
