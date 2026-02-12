using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class CashRegisterEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string OperatorUserId { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public int RegisterNumber { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningAmount { get; set; }
    public decimal? ClosingAmount { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public decimal? Discrepancy { get; set; }
    public string? DiscrepancyNotes { get; set; }
    public CashRegisterStatus Status { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    public ICollection<SaleEntity> Sales { get; set; } = new List<SaleEntity>();
}
