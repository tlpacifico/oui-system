using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class ConsignmentItemEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    public string IdentificationNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal EvaluatedValue { get; set; }
    public ConsignmentItemStatus Status { get; set; }
    public long? ConsignmentId { get; set; }
    public long? SupplierId { get; set; }

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
}
