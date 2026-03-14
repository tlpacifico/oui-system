namespace Oui.Modules.Inventory.Application.Tags;

public sealed record TagListResponse(
    Guid ExternalId,
    string Name,
    string? Color,
    int ItemCount,
    DateTime CreatedOn);

public sealed record TagDetailResponse(
    Guid ExternalId,
    string Name,
    string? Color,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);
