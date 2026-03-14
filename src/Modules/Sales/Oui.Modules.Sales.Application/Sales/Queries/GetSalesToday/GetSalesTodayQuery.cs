using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Sales.Queries.GetSalesToday;

public sealed record GetSalesTodayQuery() : IQuery<TodaySalesResponse>;
