using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionItems;

public sealed record GetReceptionItemsQuery(Guid ExternalId) : IQuery<List<EvaluationItemResponse>>;
