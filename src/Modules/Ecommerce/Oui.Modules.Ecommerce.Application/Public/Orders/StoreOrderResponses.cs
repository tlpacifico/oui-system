namespace Oui.Modules.Ecommerce.Application.Public.Orders;

public sealed record CreateOrderResponse(
    Guid ExternalId,
    string OrderNumber,
    string CustomerName,
    decimal TotalAmount,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    List<CreateOrderItemResponse> Items,
    List<string> UnavailableProducts);

public sealed record CreateOrderItemResponse(
    string ProductTitle,
    decimal Price);

public sealed record OrderStatusResponse(
    string OrderNumber,
    string Status,
    string CustomerName,
    decimal TotalAmount,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    List<OrderStatusItemResponse> Items);

public sealed record OrderStatusItemResponse(
    string ProductTitle,
    decimal Price);
