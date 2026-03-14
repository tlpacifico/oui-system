using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetFinanceReport;

public sealed record GetFinanceReportQuery(
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<FinanceReportResponse>;
