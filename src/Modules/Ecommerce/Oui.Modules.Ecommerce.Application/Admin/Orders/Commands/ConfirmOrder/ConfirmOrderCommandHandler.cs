using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.ConfirmOrder;

internal sealed class ConfirmOrderCommandHandler(EcommerceDbContext db)
    : ICommandHandler<ConfirmOrderCommand, ConfirmOrderResponse>
{
    public async Task<Result<ConfirmOrderResponse>> Handle(
        ConfirmOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await db.EcommerceOrders
            .FirstOrDefaultAsync(o => o.ExternalId == request.ExternalId, cancellationToken);

        if (order is null)
            return Result.Failure<ConfirmOrderResponse>(OrderErrors.NotFound);

        if (order.Status != EcommerceOrderStatus.Pending)
            return Result.Failure<ConfirmOrderResponse>(OrderErrors.OnlyPendingCanBeConfirmed);

        order.Status = EcommerceOrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new ConfirmOrderResponse(order.ExternalId, order.OrderNumber, "Encomenda confirmada com sucesso.");
    }
}
