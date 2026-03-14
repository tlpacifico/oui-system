using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Queries.GetStoreCreditById;

public sealed record GetStoreCreditByIdQuery(Guid ExternalId) : IQuery<StoreCreditDetailResponse>;
