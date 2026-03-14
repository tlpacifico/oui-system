using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetSettlements;

public sealed record GetSettlementsQuery(
    long? SupplierId,
    SettlementStatus? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<SettlementListResponse>;
