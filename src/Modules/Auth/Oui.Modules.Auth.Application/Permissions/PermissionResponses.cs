namespace Oui.Modules.Auth.Application.Permissions;

public sealed record PermissionResponse(
    Guid ExternalId,
    string Name,
    string Category,
    string? Description);
