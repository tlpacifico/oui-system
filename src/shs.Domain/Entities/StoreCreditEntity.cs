namespace shs.Domain.Entities;

/// <summary>
/// Represents store credit issued to a supplier.
/// Can be used as payment method in future purchases.
/// </summary>
public class StoreCreditEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Supplier who owns this store credit
    /// </summary>
    public long SupplierId { get; set; }
    public SupplierEntity Supplier { get; set; } = null!;

    /// <summary>
    /// Settlement that generated this credit (if applicable)
    /// </summary>
    public long? SourceSettlementId { get; set; }
    public SettlementEntity? SourceSettlement { get; set; }

    /// <summary>
    /// Original credit amount issued
    /// </summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>
    /// Current remaining balance
    /// </summary>
    public decimal CurrentBalance { get; set; }

    /// <summary>
    /// When credit was issued
    /// </summary>
    public DateTime IssuedOn { get; set; }
    public string? IssuedBy { get; set; }

    /// <summary>
    /// Optional expiration date
    /// </summary>
    public DateTime? ExpiresOn { get; set; }

    /// <summary>
    /// Credit status
    /// </summary>
    public StoreCreditStatus Status { get; set; }

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    // Soft delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }

    // Navigation
    public ICollection<StoreCreditTransactionEntity> Transactions { get; set; } = new List<StoreCreditTransactionEntity>();
}

public enum StoreCreditStatus
{
    /// <summary>
    /// Credit is active and can be used
    /// </summary>
    Active = 1,

    /// <summary>
    /// Credit has been fully used (balance = 0)
    /// </summary>
    FullyUsed = 2,

    /// <summary>
    /// Credit has expired
    /// </summary>
    Expired = 3,

    /// <summary>
    /// Credit was cancelled/voided
    /// </summary>
    Cancelled = 4
}
