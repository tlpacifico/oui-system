using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnById;

public sealed record GetReturnByIdQuery(Guid ExternalId) : IQuery<SupplierReturnDetailResponse>;
