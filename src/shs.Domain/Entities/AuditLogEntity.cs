namespace shs.Domain.Entities;

public class AuditLogEntity
{
    public long Id { get; set; }
    public string EntityName { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string? UserEmail { get; set; }
    public DateTime Timestamp { get; set; }
}
