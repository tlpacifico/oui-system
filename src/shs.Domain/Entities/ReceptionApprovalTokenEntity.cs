namespace shs.Domain.Entities;

public class ReceptionApprovalTokenEntity : EntityWithIdAuditable<long>
{
    public long ReceptionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public bool IsUsed { get; set; }

    // Navigation
    public ReceptionEntity Reception { get; set; } = null!;
}
