using shs.Domain.Entities;
using shs.Domain.Enums;

namespace Oui.Modules.Sales.Application.Settlements;

public sealed record CalculateSettlementResponse(
    long SupplierId,
    string SupplierName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    int ItemCount,
    decimal TotalSalesAmount,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    decimal StoreCreditAmount,
    decimal CashRedemptionAmount,
    decimal NetAmountToSupplier,
    decimal StoreCommissionAmount);

public sealed record CreateSettlementResponse(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalSalesAmount,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    decimal StoreCreditAmount,
    decimal CashRedemptionAmount,
    decimal StoreCommissionAmount,
    decimal NetAmountToSupplier,
    SettlementStatus Status,
    int ItemCount,
    string? Notes,
    DateTime CreatedOn,
    string? CreatedBy);

public sealed record SettlementListResponse(
    int Total,
    int Page,
    int PageSize,
    List<SettlementListItem> Data);

public sealed record SettlementListItem(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    string SupplierInitial,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalSalesAmount,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    decimal StoreCreditAmount,
    decimal CashRedemptionAmount,
    decimal StoreCommissionAmount,
    decimal NetAmountToSupplier,
    SettlementStatus Status,
    int ItemCount,
    DateTime? PaidOn,
    string? PaidBy,
    DateTime CreatedOn,
    string? CreatedBy);

public sealed record SettlementDetailResponse(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    string? SupplierEmail,
    string? SupplierPhone,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal TotalSalesAmount,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    decimal StoreCreditAmount,
    decimal CashRedemptionAmount,
    decimal StoreCommissionAmount,
    decimal NetAmountToSupplier,
    SettlementStatus Status,
    DateTime? PaidOn,
    string? PaidBy,
    string? Notes,
    DateTime CreatedOn,
    string? CreatedBy,
    SettlementStoreCreditInfo? StoreCredit,
    List<SettlementItemInfo> Items);

public sealed record SettlementStoreCreditInfo(
    Guid ExternalId,
    decimal OriginalAmount,
    decimal CurrentBalance,
    StoreCreditStatus Status,
    DateTime IssuedOn);

public sealed record SettlementItemInfo(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string BrandName,
    decimal EvaluatedPrice,
    decimal FinalPrice,
    DateTime? SaleDate);

public sealed record ProcessPaymentResponse(
    Guid ExternalId,
    SettlementStatus Status,
    DateTime? PaidOn,
    string? PaidBy,
    string Message);

public sealed record CancelSettlementResponse(
    Guid ExternalId,
    SettlementStatus Status,
    string Message);

public sealed record PendingSettlementGroup(
    long? SupplierId,
    string SupplierName,
    string SupplierInitial,
    int ItemCount,
    decimal TotalSalesAmount,
    List<PendingSettlementItem> Items);

public sealed record PendingSettlementItem(
    long ItemId,
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string BrandName,
    decimal EvaluatedPrice,
    decimal? FinalSalePrice,
    decimal? CommissionPercentage,
    decimal? CommissionAmount,
    long? SupplierId,
    string SupplierName,
    string SupplierInitial,
    DateTime? UpdatedOn);
