using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetStoreProducts;

public sealed record GetStoreProductsQuery(
    string? Search,
    string? Brand,
    string? Category,
    string? Size,
    string? Color,
    string? Condition,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Sort,
    int Page,
    int PageSize) : IQuery<StorePagedResult<StoreProductListResponse>>;
