using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Orders.Queries.GetOrderStatus;

internal sealed class GetOrderStatusQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetOrderStatusQuery, OrderStatusResponse>
{
    public async Task<Result<OrderStatusResponse>> Handle(
        GetOrderStatusQuery request, CancellationToken cancellationToken)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == request.ExternalId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderStatusResponse>(StoreOrderErrors.NotFound);

        return new OrderStatusResponse(
            order.OrderNumber,
            order.Status.ToString(),
            order.CustomerName,
            order.TotalAmount,
            order.ReservedAt,
            order.ExpiresAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.CancelledAt,
            order.CancellationReason,
            order.Items.Select(i => new OrderStatusItemResponse(i.ProductTitle, i.Price)).ToList());
    }
}
