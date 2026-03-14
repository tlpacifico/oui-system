namespace Oui.Modules.Ecommerce.Application.Admin.Products;

public sealed record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record ProductListResponse(
    Guid ExternalId,
    string Slug,
    string Title,
    decimal Price,
    string BrandName,
    string? CategoryName,
    string? Size,
    string? Color,
    string Status,
    DateTime? PublishedAt,
    DateTime? UnpublishedAt,
    string? PrimaryPhotoUrl);

public sealed record ProductDetailResponse(
    Guid ExternalId,
    string Slug,
    string Title,
    string? Description,
    decimal Price,
    string BrandName,
    string? CategoryName,
    string? Size,
    string? Color,
    string Condition,
    string? Composition,
    string Status,
    DateTime? PublishedAt,
    DateTime? UnpublishedAt,
    List<ProductPhotoResponse> Photos);

public sealed record ProductPhotoResponse(
    Guid ExternalId,
    string FilePath,
    string? ThumbnailPath,
    int DisplayOrder,
    bool IsPrimary);

public sealed record PublishItemResponse(
    Guid ExternalId,
    string Slug,
    string Title,
    decimal Price,
    string Status);

public sealed record PublishBatchResponse(
    List<PublishBatchItemResponse> Published,
    List<PublishBatchErrorResponse> Errors,
    int TotalPublished,
    int TotalErrors);

public sealed record PublishBatchItemResponse(
    Guid ExternalId,
    string Slug,
    string Title);

public sealed record PublishBatchErrorResponse(
    Guid ExternalId,
    string Error);

public sealed record UpdateProductResponse(
    Guid ExternalId,
    string Title,
    decimal Price);

public sealed record UnpublishProductResponse(
    Guid ExternalId,
    string Message);

public sealed record UploadProductPhotoResponse(
    Guid ExternalId,
    string FilePath,
    string? ThumbnailPath,
    int DisplayOrder,
    bool IsPrimary);
