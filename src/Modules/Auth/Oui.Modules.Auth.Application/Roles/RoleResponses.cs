namespace Oui.Modules.Auth.Application.Roles;

public sealed record RoleListResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    bool IsSystemRole,
    int UserCount,
    int PermissionCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);

public sealed record RoleDetailResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    bool IsSystemRole,
    int UserCount,
    int PermissionCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy,
    List<RolePermissionSummary> Permissions);

public sealed record RolePermissionSummary(
    Guid ExternalId,
    string Name,
    string Category,
    string? Description);
