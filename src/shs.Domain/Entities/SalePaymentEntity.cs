using shs.Domain.Enums;

namespace shs.Domain.Entities;

public class SalePaymentEntity
{
    public long Id { get; set; }
    public long SaleId { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public string? Reference { get; set; }

    public SaleEntity Sale { get; set; } = null!;
}
