namespace shs.Domain.Entities;

/// <summary>
/// Tracks individual transactions against store credit.
/// Provides audit trail of credit usage.
/// </summary>
public class StoreCreditTransactionEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Store credit this transaction belongs to
    /// </summary>
    public long StoreCreditId { get; set; }
    public StoreCreditEntity StoreCredit { get; set; } = null!;

    /// <summary>
    /// Sale where credit was used (if applicable)
    /// </summary>
    public long? SaleId { get; set; }
    public SaleEntity? Sale { get; set; }

    /// <summary>
    /// Transaction amount (positive for credit, negative for debit)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Balance after this transaction
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Type of transaction
    /// </summary>
    public StoreCreditTransactionType TransactionType { get; set; }

    /// <summary>
    /// When transaction occurred
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Who processed this transaction
    /// </summary>
    public string? ProcessedBy { get; set; }

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
}

public enum StoreCreditTransactionType
{
    /// <summary>
    /// Credit issued (positive amount)
    /// </summary>
    Issue = 1,

    /// <summary>
    /// Credit used in a purchase (negative amount)
    /// </summary>
    Use = 2,

    /// <summary>
    /// Manual adjustment (can be positive or negative)
    /// </summary>
    Adjustment = 3,

    /// <summary>
    /// Credit expired (negative amount to zero balance)
    /// </summary>
    Expiration = 4,

    /// <summary>
    /// Credit cancelled/voided (negative amount to zero balance)
    /// </summary>
    Cancellation = 5
}
