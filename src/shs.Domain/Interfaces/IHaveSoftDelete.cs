namespace shs.Domain.Interfaces;

public interface IHaveSoftDelete
{
    bool IsDeleted { get; set; }
    string? DeletedBy { get; set; }
    DateTime? DeletedOn { get; set; }
}
