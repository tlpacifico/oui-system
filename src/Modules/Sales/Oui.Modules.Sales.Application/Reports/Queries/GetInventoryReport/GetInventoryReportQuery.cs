using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetInventoryReport;

public sealed record GetInventoryReportQuery() : IQuery<InventoryReportResponse>;
