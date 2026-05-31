namespace Oui.Modules.Inventory.Application.Colors;

public sealed record ColorListResponse(
    Guid ExternalId,
    string Name,
    string? HexCode,
    int ItemCount,
    DateTime CreatedOn);

public sealed record ColorDetailResponse(
    Guid ExternalId,
    string Name,
    string? HexCode,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);
