namespace shs.Domain.Entities;

/// <summary>
/// Tracks credits and debits to supplier's cash redemption balance (PorcInDinheiro).
/// Positive amount = credit from settlement. Negative amount = cash redemption.
/// </summary>
public class SupplierCashBalanceTransactionEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    public long SupplierId { get; set; }
    public SupplierEntity Supplier { get; set; } = null!;

    /// <summary>
    /// Positive = credit from settlement, Negative = cash redemption
    /// </summary>
    public decimal Amount { get; set; }

    public SupplierCashBalanceTransactionType TransactionType { get; set; }

    /// <summary>
    /// Settlement that generated this credit (when TransactionType = SettlementCredit)
    /// </summary>
    public long? SettlementId { get; set; }
    public SettlementEntity? Settlement { get; set; }

    public DateTime TransactionDate { get; set; }
    public string? ProcessedBy { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}

public enum SupplierCashBalanceTransactionType
{
    /// <summary>
    /// Credit from settlement (PorcInDinheiro amount)
    /// </summary>
    SettlementCredit = 1,

    /// <summary>
    /// Cash redemption - supplier withdrew money
    /// </summary>
    CashRedemption = 2
}
