namespace Oui.Modules.Auth.Application.Users;

public sealed record UserListResponse(
    Guid ExternalId,
    string Email,
    string? DisplayName,
    DateTime CreatedOn,
    List<UserRoleSummary> Roles,
    int RoleCount);

public sealed record UserRoleSummary(Guid ExternalId, string Name);

public sealed record UserDetailResponse(
    Guid ExternalId,
    string Email,
    string? DisplayName,
    DateTime CreatedOn,
    List<UserRoleDetail> Roles,
    int RoleCount);

public sealed record UserRoleDetail(Guid ExternalId, string Name, DateTime AssignedOn, string? AssignedBy);
