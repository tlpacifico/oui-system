using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Dashboard.Queries.GetDashboard;

public sealed record GetDashboardQuery(string Period = "today") : IQuery<DashboardResponse>;
