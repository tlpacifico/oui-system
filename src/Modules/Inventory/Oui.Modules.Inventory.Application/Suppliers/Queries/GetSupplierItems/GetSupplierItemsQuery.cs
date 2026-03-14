using Oui.Modules.Inventory.Application.Items;
using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierItems;

public sealed record GetSupplierItemsQuery(
    Guid ExternalId,
    string? Status,
    int Page,
    int PageSize) : IQuery<PagedResult<SupplierItemResponse>>;
