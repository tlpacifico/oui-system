using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnableItems;

public sealed record GetReturnableItemsQuery(Guid SupplierExternalId) : IQuery<List<ReturnableItemResponse>>;
