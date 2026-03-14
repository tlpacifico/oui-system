namespace Oui.Modules.Ecommerce.Application.Admin.Orders;

public sealed record OrderListResponse(
    Guid ExternalId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Status,
    decimal TotalAmount,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    DateTime? ConfirmedAt,
    int ItemCount);

public sealed record OrderDetailResponse(
    Guid ExternalId,
    string OrderNumber,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Status,
    decimal TotalAmount,
    string? Notes,
    DateTime ReservedAt,
    DateTime ExpiresAt,
    DateTime? ConfirmedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    List<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ExternalId,
    string ProductTitle,
    decimal Price);

public sealed record ConfirmOrderResponse(
    Guid ExternalId,
    string OrderNumber,
    string Message);

public sealed record CancelOrderResponse(
    Guid ExternalId,
    string OrderNumber,
    string Message);
