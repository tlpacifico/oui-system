using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrderById;

internal sealed class GetOrderByIdQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetOrderByIdQuery, OrderDetailResponse>
{
    public async Task<Result<OrderDetailResponse>> Handle(
        GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == request.ExternalId, cancellationToken);

        if (order is null)
            return Result.Failure<OrderDetailResponse>(OrderErrors.NotFound);

        return new OrderDetailResponse(
            order.ExternalId,
            order.OrderNumber,
            order.CustomerName,
            order.CustomerEmail,
            order.CustomerPhone,
            order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            order.ReservedAt,
            order.ExpiresAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.CancelledAt,
            order.CancellationReason,
            order.Items.Select(i => new OrderItemResponse(
                i.ExternalId,
                i.ProductTitle,
                i.Price)).ToList());
    }
}
