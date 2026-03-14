using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Application.Admin.Products;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrders;

internal sealed class GetOrdersQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetOrdersQuery, PagedResult<OrderListResponse>>
{
    public async Task<Result<PagedResult<OrderListResponse>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = db.EcommerceOrders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EcommerceOrderStatus>(request.Status, true, out var statusEnum))
            query = query.Where(o => o.Status == statusEnum);

        var totalCount = await query.CountAsync(cancellationToken);
        var orders = await query
            .OrderByDescending(o => o.ReservedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListResponse(
                o.ExternalId,
                o.OrderNumber,
                o.CustomerName,
                o.CustomerEmail,
                o.CustomerPhone,
                o.Status.ToString(),
                o.TotalAmount,
                o.ReservedAt,
                o.ExpiresAt,
                o.ConfirmedAt,
                o.Items.Count))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderListResponse>(
            orders,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling((double)totalCount / request.PageSize));
    }
}
