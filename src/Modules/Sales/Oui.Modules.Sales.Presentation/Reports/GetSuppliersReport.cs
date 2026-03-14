using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Reports.Queries.GetSuppliersReport;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Reports;

internal sealed class GetSuppliersReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/reports/suppliers", async (DateTime? startDate, DateTime? endDate, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSuppliersReportQuery(startDate, endDate), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Reports");
    }
}
