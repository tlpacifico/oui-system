using shs.Domain.Enums;

namespace shs.Domain.Entities;

public class SalePaymentEntity
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }

    /// <summary>
    /// When PaymentMethod is StoreCredit, identifies which supplier's credit was used
    /// </summary>
    public long? SupplierId { get; set; }
    public SupplierEntity? Supplier { get; set; }

    /// <summary>
    /// Store credit used (when PaymentMethod is StoreCredit)
    /// </summary>
    public long? StoreCreditId { get; set; }
    public StoreCreditEntity? StoreCredit { get; set; }

    public SaleEntity Sale { get; set; } = null!;
}
