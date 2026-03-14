using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Commands.ProcessSale;

public sealed record ProcessSaleCommand(
    Guid CashRegisterId,
    SaleItemInput[] Items,
    SalePaymentInput[] Payments,
    decimal? DiscountPercentage,
    string? DiscountReason,
    Guid? CustomerExternalId,
    string? Notes,
    string UserId) : ICommand<ProcessSaleResponse>;

public sealed record SaleItemInput(
    Guid ItemExternalId,
    decimal DiscountAmount);

public sealed record SalePaymentInput(
    string Method,
    decimal Amount,
    string? Reference,
    long? SupplierId);
