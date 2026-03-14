using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetSettlementById;

public sealed record GetSettlementByIdQuery(Guid ExternalId) : IQuery<SettlementDetailResponse>;
