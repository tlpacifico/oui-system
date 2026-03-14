using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Sales.Queries.GetSaleById;

public sealed record GetSaleByIdQuery(Guid ExternalId) : IQuery<SaleDetailResponse>;
