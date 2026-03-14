using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Queries.GetPendingSettlementItems;

public sealed record GetPendingSettlementItemsQuery(
    long? SupplierId,
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<List<PendingSettlementGroup>>;
