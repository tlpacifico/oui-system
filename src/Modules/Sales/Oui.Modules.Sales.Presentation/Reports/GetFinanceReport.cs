using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Reports.Queries.GetFinanceReport;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Reports;

internal sealed class GetFinanceReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/finance", async (DateTime? startDate, DateTime? endDate, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFinanceReportQuery(startDate, endDate), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Reports");
    }
}
