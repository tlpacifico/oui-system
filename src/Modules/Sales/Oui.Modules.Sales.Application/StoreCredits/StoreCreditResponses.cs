using shs.Domain.Entities;
using shs.Domain.Enums;

namespace Oui.Modules.Sales.Application.StoreCredits;

public sealed record SupplierStoreCreditsResponse(
    long SupplierId,
    decimal TotalActiveBalance,
    List<StoreCreditListItem> Credits);

public sealed record StoreCreditListItem(
    Guid ExternalId,
    decimal OriginalAmount,
    decimal CurrentBalance,
    StoreCreditStatus Status,
    DateTime IssuedOn,
    string? IssuedBy,
    DateTime? ExpiresOn,
    string? Notes,
    StoreCreditSourceSettlement? SourceSettlement);

public sealed record StoreCreditSourceSettlement(
    Guid ExternalId,
    DateTime PeriodStart,
    DateTime PeriodEnd);

public sealed record SupplierStoreCreditBalanceResponse(
    long SupplierId,
    decimal TotalActiveBalance);

public sealed record StoreCreditDetailResponse(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    decimal OriginalAmount,
    decimal CurrentBalance,
    StoreCreditStatus Status,
    DateTime IssuedOn,
    string? IssuedBy,
    DateTime? ExpiresOn,
    string? Notes,
    StoreCreditDetailSettlement? SourceSettlement,
    int TransactionCount,
    StoreCreditLastTransaction? LastTransaction);

public sealed record StoreCreditDetailSettlement(
    Guid ExternalId,
    decimal TotalSalesAmount,
    DateTime PeriodStart,
    DateTime PeriodEnd);

public sealed record StoreCreditLastTransaction(
    DateTime TransactionDate,
    decimal Amount,
    StoreCreditTransactionType TransactionType);

public sealed record StoreCreditTransactionsResponse(
    Guid StoreCreditId,
    decimal CurrentBalance,
    List<StoreCreditTransactionItem> Transactions);

public sealed record StoreCreditTransactionItem(
    Guid ExternalId,
    decimal Amount,
    decimal BalanceAfter,
    StoreCreditTransactionType TransactionType,
    DateTime TransactionDate,
    string? ProcessedBy,
    string? Notes,
    StoreCreditTransactionSale? Sale);

public sealed record StoreCreditTransactionSale(
    Guid ExternalId,
    string SaleNumber,
    decimal TotalAmount);

public sealed record IssueStoreCreditResponse(
    Guid ExternalId,
    long SupplierId,
    string SupplierName,
    decimal OriginalAmount,
    decimal CurrentBalance,
    StoreCreditStatus Status,
    DateTime IssuedOn,
    string? IssuedBy);

public sealed record UseStoreCreditResponse(
    Guid ExternalId,
    decimal AmountUsed,
    decimal RemainingBalance,
    StoreCreditStatus Status);

public sealed record UseStoreCreditBySupplierResponse(
    long SupplierId,
    string SupplierName,
    decimal TotalUsed,
    List<UsedCreditInfo> CreditsUsed);

public sealed record UsedCreditInfo(
    Guid ExternalId,
    decimal AmountUsed,
    decimal RemainingBalance);

public sealed record AdjustStoreCreditResponse(
    Guid ExternalId,
    decimal OldBalance,
    decimal AdjustmentAmount,
    decimal NewBalance,
    StoreCreditStatus Status,
    string? Reason);

public sealed record CancelStoreCreditResponse(
    Guid ExternalId,
    StoreCreditStatus Status,
    decimal CancelledBalance,
    string Message);
