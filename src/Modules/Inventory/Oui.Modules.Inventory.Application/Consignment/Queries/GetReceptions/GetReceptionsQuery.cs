using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptions;

public sealed record GetReceptionsQuery(
    string? Status,
    Guid? SupplierExternalId,
    string? Search,
    int Page,
    int PageSize) : IQuery<ReceptionPagedResult>;
