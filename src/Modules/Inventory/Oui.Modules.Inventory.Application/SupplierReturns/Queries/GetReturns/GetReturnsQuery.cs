using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturns;

public sealed record GetReturnsQuery(
    Guid? SupplierExternalId,
    string? Search,
    int Page,
    int PageSize) : IQuery<SupplierReturnPagedResult>;
