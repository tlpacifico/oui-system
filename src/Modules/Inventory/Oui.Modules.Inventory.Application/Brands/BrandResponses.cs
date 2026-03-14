namespace Oui.Modules.Inventory.Application.Brands;

public sealed record BrandListResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    string? LogoUrl,
    int ItemCount,
    DateTime CreatedOn);

public sealed record BrandDetailResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    string? LogoUrl,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);
