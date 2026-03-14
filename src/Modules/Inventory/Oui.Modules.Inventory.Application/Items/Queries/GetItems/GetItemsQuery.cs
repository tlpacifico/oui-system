using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Queries.GetItems;

public sealed record GetItemsQuery(
    string? Search,
    long? BrandId,
    string? Status,
    int Page,
    int PageSize) : IQuery<PagedResult<ItemListItemResponse>>;
