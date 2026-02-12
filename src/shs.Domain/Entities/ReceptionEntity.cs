using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class ReceptionEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public long SupplierId { get; set; }
    public DateTime ReceptionDate { get; set; }
    public int ItemCount { get; set; } // Initial count of items received
    public ReceptionStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime? EvaluatedAt { get; set; }
    public string? EvaluatedBy { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation
    public SupplierEntity Supplier { get; set; } = null!;
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
