using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class SupplierEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty; // Format: +351XXXXXXXXX
    public string? TaxNumber { get; set; } // NIF (optional)
    public string Initial { get; set; } = string.Empty; // Single letter for ID generation (e.g., "M")
    public string? Notes { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation
    public ICollection<ReceptionEntity> Receptions { get; set; } = new List<ReceptionEntity>();
    public ICollection<ItemEntity> Items { get; set; } = new List<ItemEntity>();
}
