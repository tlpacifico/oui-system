using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.ConfirmOrder;

public sealed record ConfirmOrderCommand(Guid ExternalId) : ICommand<ConfirmOrderResponse>;
