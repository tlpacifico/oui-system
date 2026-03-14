using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionById;

public sealed record GetReceptionByIdQuery(Guid ExternalId) : IQuery<ReceptionDetailResponse>;
