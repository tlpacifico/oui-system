using shs.Domain.Entities;
using shs.Domain.Enums;

namespace Oui.Modules.Sales.Application.CashRedemptions;

public sealed record SupplierCashBalanceResponse(
    long SupplierId,
    string SupplierName,
    decimal AvailableBalance,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage);

public sealed record SupplierCashRedemptionHistoryResponse(
    long SupplierId,
    string SupplierName,
    decimal CurrentBalance,
    int Total,
    int Page,
    int PageSize,
    int TotalPages,
    List<CashBalanceTransactionItem> Transactions);

public sealed record CashBalanceTransactionItem(
    Guid ExternalId,
    decimal Amount,
    SupplierCashBalanceTransactionType TransactionType,
    DateTime TransactionDate,
    string? ProcessedBy,
    string? Notes,
    string? SettlementPeriod);

public sealed record ProcessCashRedemptionResponse(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    decimal AmountRedeemed,
    decimal PreviousBalance,
    decimal NewBalance,
    DateTime TransactionDate,
    string? ProcessedBy,
    string Message);
