using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Queries.GetItems;

public sealed record GetItemsQuery(
    string? Search,
    Guid? BrandExternalId,
    Guid? CategoryExternalId,
    Guid? SupplierExternalId,
    Guid? ColorExternalId,
    string? Size,
    string? Status,
    string? Condition,
    string? AcquisitionType,
    decimal? MinPrice,
    decimal? MaxPrice,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    string? SortBy,
    string? SortDir,
    int Page,
    int PageSize) : IQuery<PagedResult<ItemListItemResponse>>;
