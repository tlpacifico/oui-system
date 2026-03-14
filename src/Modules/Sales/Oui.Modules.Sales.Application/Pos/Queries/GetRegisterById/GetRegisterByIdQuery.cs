using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetRegisterById;

public sealed record GetRegisterByIdQuery(Guid ExternalId) : IQuery<RegisterDetailResponse>;
