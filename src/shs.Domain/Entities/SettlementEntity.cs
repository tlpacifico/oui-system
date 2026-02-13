namespace shs.Domain.Entities;

/// <summary>
/// Represents a financial settlement with a consignment supplier.
/// Tracks the payment of commissions for sold items.
/// </summary>
public class SettlementEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }

    /// <summary>
    /// Supplier being paid
    /// </summary>
    public long SupplierId { get; set; }
    public SupplierEntity Supplier { get; set; } = null!;

    /// <summary>
    /// Settlement period (inclusive)
    /// </summary>
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Total sales value of all items in this settlement
    /// </summary>
    public decimal TotalSalesAmount { get; set; }

    /// <summary>
    /// Commission percentage to supplier (40% for cash, 50% for store credit)
    /// </summary>
    public decimal CommissionPercentage { get; set; }

    /// <summary>
    /// Amount the store keeps (store commission)
    /// </summary>
    public decimal StoreCommissionAmount { get; set; }

    /// <summary>
    /// Net amount payable to supplier
    /// </summary>
    public decimal NetAmountToSupplier { get; set; }

    /// <summary>
    /// Payment method: Cash or StoreCredit
    /// </summary>
    public SettlementPaymentMethod PaymentMethod { get; set; }

    /// <summary>
    /// Settlement status
    /// </summary>
    public SettlementStatus Status { get; set; }

    /// <summary>
    /// When payment was processed
    /// </summary>
    public DateTime? PaidOn { get; set; }
    public string? PaidBy { get; set; }

    /// <summary>
    /// Store credit record if PaymentMethod is StoreCredit
    /// </summary>
    public long? StoreCreditId { get; set; }
    public StoreCreditEntity? StoreCredit { get; set; }

    /// <summary>
    /// Optional notes about this settlement
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
    public ICollection<SaleItemEntity> SaleItems { get; set; } = new List<SaleItemEntity>();
}

public enum SettlementPaymentMethod
{
    /// <summary>
    /// Cash payment - 40% to supplier, 60% to store
    /// </summary>
    Cash = 1,

    /// <summary>
    /// Store credit - 50% to supplier as credit, 50% to store
    /// </summary>
    StoreCredit = 2
}

public enum SettlementStatus
{
    /// <summary>
    /// Settlement created but not yet paid
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Payment completed
    /// </summary>
    Paid = 2,

    /// <summary>
    /// Settlement cancelled
    /// </summary>
    Cancelled = 3
}
