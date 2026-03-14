using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Queries.GetItemById;

public sealed record GetItemByIdQuery(Guid ExternalId) : IQuery<ItemDetailResponse>;
