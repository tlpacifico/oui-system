using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid ExternalId, string? Reason) : ICommand<CancelOrderResponse>;
