using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Orders.Queries.GetOrderStatus;

public sealed record GetOrderStatusQuery(Guid ExternalId) : IQuery<OrderStatusResponse>;
