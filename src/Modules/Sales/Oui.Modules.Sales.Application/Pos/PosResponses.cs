namespace Oui.Modules.Sales.Application.Pos;

public sealed record RegisterResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal? ExpectedAmount,
    decimal? Discrepancy,
    string Status,
    int SalesCount,
    decimal SalesTotal);

public sealed record CloseRegisterResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime ClosedAt,
    int SalesCount,
    decimal TotalRevenue,
    Dictionary<string, decimal> SalesByPaymentMethod,
    decimal ExpectedCash,
    decimal CountedCash,
    decimal Discrepancy);

public sealed record RegisterDetailResponse(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorUserId,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    decimal OpeningAmount,
    decimal? ClosingAmount,
    decimal? ExpectedAmount,
    decimal? Discrepancy,
    string? DiscrepancyNotes,
    string Status,
    int SalesCount,
    decimal SalesTotal,
    int ItemsCount,
    Dictionary<string, decimal> SalesByPaymentMethod,
    List<RegisterSaleInfo> Sales,
    DateTime CreatedOn);

public sealed record RegisterSaleInfo(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal TotalAmount,
    int ItemCount,
    string Status);

public sealed record RegisterStatusItem(
    Guid ExternalId,
    int RegisterNumber,
    string OperatorName,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    string Status,
    int SalesCount,
    decimal SalesTotal,
    decimal? Discrepancy);

public sealed record AllRegistersStatusResponse(
    int OpenCount,
    int ClosedCount,
    List<RegisterStatusItem> Registers);

public sealed record CurrentRegisterResponse(
    bool Open,
    RegisterResponse? Register);

public sealed record ProcessSaleResponse(
    Guid ExternalId,
    string SaleNumber,
    DateTime SaleDate,
    decimal Subtotal,
    decimal DiscountPercentage,
    decimal DiscountAmount,
    decimal TotalAmount,
    decimal Change,
    int ItemCount,
    string CashierName);

public sealed record PosSupplierResponse(
    long Id,
    string Name,
    string Initial);
