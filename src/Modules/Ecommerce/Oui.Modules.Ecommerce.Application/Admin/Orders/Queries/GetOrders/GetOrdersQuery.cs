using Oui.Modules.Ecommerce.Application.Admin.Products;
using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery(
    string? Status,
    int Page,
    int PageSize) : IQuery<PagedResult<OrderListResponse>>;
