namespace Oui.Modules.Sales.Application.Sales;

public sealed record SaleDetailResponse(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    string? DiscountReason,
    string Status,
    string? Notes,
    string CashierName,
    int RegisterNumber,
    List<SaleItemDetail> Items,
    List<SalePaymentDetail> Payments,
    DateTime CreatedOn);

public sealed record SaleItemDetail(
    Guid ItemExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string? SupplierName,
    decimal UnitPrice,
    decimal DiscountAmount,
    decimal FinalPrice);

public sealed record SalePaymentDetail(
    string Method,
    decimal Amount,
    string? Reference);

public sealed record TodaySalesResponse(
    int SalesCount,
    decimal TotalRevenue,
    decimal AverageTicket,
    int TotalItems,
    Dictionary<string, PaymentMethodSummary> ByPaymentMethod,
    List<SaleListItem> RecentSales);

public sealed record PaymentMethodSummary(int Count, decimal Total);

public sealed record SaleListItem(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    int ItemCount,
    string Status,
    string PaymentMethods);

public sealed record SalesPagedResult(
    List<SaleListItem> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
