namespace shs.Domain.Entities;

public class UserEntity
{
    public long Id { get; set; }
    public Guid ExternalId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTime CreatedOn { get; set; }
}
