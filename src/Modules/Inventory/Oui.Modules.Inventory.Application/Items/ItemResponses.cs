namespace Oui.Modules.Inventory.Application.Items;

public sealed record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record ItemListItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    decimal EvaluatedPrice,
    string Status,
    string? PrimaryPhotoUrl,
    DateTime CreatedOn,
    Guid? EcommerceProductExternalId,
    string? EcommerceProductSlug,
    string? EcommerceProductStatus);

public sealed record ItemDetailResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string? Description,
    BrandInfo Brand,
    CategoryInfo? Category,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    decimal? FinalSalePrice,
    string Status,
    string AcquisitionType,
    string Origin,
    SupplierInfo? Supplier,
    decimal CommissionPercentage,
    decimal? CommissionAmount,
    bool IsRejected,
    string? RejectionReason,
    DateTime? SoldAt,
    int DaysInStock,
    List<TagInfo> Tags,
    List<PhotoInfo> Photos,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);

public sealed record CreateConsignmentItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Status,
    DateTime CreatedAt);

public sealed record BrandInfo(long Id, string Name);
public sealed record CategoryInfo(long Id, string Name);
public sealed record SupplierInfo(long Id, string Name);
public sealed record TagInfo(long Id, string Name, string? Color);
public sealed record PhotoInfo(Guid ExternalId, string FilePath, string? ThumbnailPath, int DisplayOrder, bool IsPrimary);
