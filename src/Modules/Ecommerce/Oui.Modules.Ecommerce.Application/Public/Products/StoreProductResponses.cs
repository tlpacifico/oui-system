namespace Oui.Modules.Ecommerce.Application.Public.Products;

public sealed record StoreProductListResponse(
    string Slug,
    string Title,
    decimal Price,
    string BrandName,
    string? CategoryName,
    string? Size,
    string? Color,
    string Condition,
    string? PrimaryPhotoUrl);

public sealed record StoreProductDetailResponse(
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
    List<StoreProductPhotoResponse> Photos);

public sealed record StoreProductPhotoResponse(
    string Url,
    string? ThumbnailUrl,
    bool IsPrimary);

public sealed record StorePagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
