using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Queries.CalculateSettlement;

public sealed record CalculateSettlementQuery(
    long SupplierId,
    DateTime PeriodStart,
    DateTime PeriodEnd) : IQuery<CalculateSettlementResponse>;
