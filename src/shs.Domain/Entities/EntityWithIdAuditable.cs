namespace shs.Domain.Entities;

public abstract class EntityWithIdAuditable<TId> where TId : struct
{
    public TId Id { get; set; }
    public Guid ExternalId { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
}
