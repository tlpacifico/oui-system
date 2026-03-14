using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(Guid ExternalId) : IQuery<OrderDetailResponse>;
