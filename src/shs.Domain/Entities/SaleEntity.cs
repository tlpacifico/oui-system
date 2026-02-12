using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class SaleEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string SaleNumber { get; set; } = string.Empty;
    public long CashRegisterId { get; set; }
    public long? CustomerId { get; set; }
    public DateTime SaleDate { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? DiscountReason { get; set; }
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    public CashRegisterEntity CashRegister { get; set; } = null!;
    public ICollection<SaleItemEntity> Items { get; set; } = new List<SaleItemEntity>();
    public ICollection<SalePaymentEntity> Payments { get; set; } = new List<SalePaymentEntity>();
}
