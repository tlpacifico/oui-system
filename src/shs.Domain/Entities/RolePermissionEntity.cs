namespace shs.Domain.Entities;

public class RolePermissionEntity
{
    public long Id { get; set; }
    public long RoleId { get; set; }
    public long PermissionId { get; set; }
    public DateTime GrantedOn { get; set; }
    public string? GrantedBy { get; set; }

    public RoleEntity Role { get; set; } = null!;
    public PermissionEntity Permission { get; set; } = null!;
}
