using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    List<string> ProductSlugs,
    string? Notes) : ICommand<CreateOrderResponse>;
