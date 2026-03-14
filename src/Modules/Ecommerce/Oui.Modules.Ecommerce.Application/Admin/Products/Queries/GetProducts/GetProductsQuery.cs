using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    string? Status,
    string? Search,
    int Page,
    int PageSize) : IQuery<PagedResult<ProductListResponse>>;
