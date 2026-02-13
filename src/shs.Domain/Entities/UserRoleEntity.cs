namespace shs.Domain.Entities;

public class UserRoleEntity
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long RoleId { get; set; }
    public DateTime AssignedOn { get; set; }
    public string? AssignedBy { get; set; }

    public UserEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
}
