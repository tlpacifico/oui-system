using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Reports.Queries.GetSalesReport;

public sealed record GetSalesReportQuery(
    DateTime? StartDate,
    DateTime? EndDate,
    long? BrandId,
    long? CategoryId) : IQuery<SalesReportResponse>;
