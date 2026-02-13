using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class SupplierReturnEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public long SupplierId { get; set; }
    public DateTime ReturnDate { get; set; }
    public int ItemCount { get; set; }
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation
    public SupplierEntity Supplier { get; set; } = null!;
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
