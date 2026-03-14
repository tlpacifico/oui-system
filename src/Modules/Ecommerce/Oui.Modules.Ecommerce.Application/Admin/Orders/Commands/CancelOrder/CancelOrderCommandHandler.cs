using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler(EcommerceDbContext db)
    : ICommandHandler<CancelOrderCommand, CancelOrderResponse>
{
    public async Task<Result<CancelOrderResponse>> Handle(
        CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == request.ExternalId, cancellationToken);

        if (order is null)
            return Result.Failure<CancelOrderResponse>(OrderErrors.NotFound);

        if (order.Status == EcommerceOrderStatus.Completed || order.Status == EcommerceOrderStatus.Cancelled)
            return Result.Failure<CancelOrderResponse>(OrderErrors.CannotBeCancelled);

        order.Status = EcommerceOrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = request.Reason ?? "Cancelado pelo staff";

        // Release reserved products back to Published
        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var reservedProducts = await db.EcommerceProducts
            .Where(p => productIds.Contains(p.Id) && p.Status == EcommerceProductStatus.Reserved)
            .ToListAsync(cancellationToken);

        foreach (var product in reservedProducts)
            product.Status = EcommerceProductStatus.Published;

        await db.SaveChangesAsync(cancellationToken);

        return new CancelOrderResponse(order.ExternalId, order.OrderNumber, "Encomenda cancelada.");
    }
}
