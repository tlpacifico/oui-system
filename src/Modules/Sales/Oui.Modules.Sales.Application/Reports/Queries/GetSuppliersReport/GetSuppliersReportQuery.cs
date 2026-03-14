using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetSuppliersReport;

public sealed record GetSuppliersReportQuery(
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<SuppliersReportResponse>;
