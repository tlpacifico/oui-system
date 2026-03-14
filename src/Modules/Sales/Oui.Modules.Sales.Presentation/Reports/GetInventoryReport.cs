using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Reports.Queries.GetInventoryReport;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Reports;

internal sealed class GetInventoryReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/inventory", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetInventoryReportQuery(), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Reports");
    }
}
