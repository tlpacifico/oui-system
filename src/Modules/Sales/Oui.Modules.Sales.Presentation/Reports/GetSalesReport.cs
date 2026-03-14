using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Reports.Queries.GetSalesReport;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Reports;

internal sealed class GetSalesReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/sales", async (DateTime? startDate, DateTime? endDate, long? brandId, long? categoryId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSalesReportQuery(startDate, endDate, brandId, categoryId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Reports");
    }
}
