namespace shs.Domain.Entities;

public class PermissionEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "inventory.items.view"
    public string Category { get; set; } = string.Empty; // e.g., "inventory", "pos", "admin"
    public string? Description { get; set; }
    public DateTime CreatedOn { get; set; }

    public ICollection<RolePermissionEntity> RolePermissions { get; set; } = new List<RolePermissionEntity>();
}
