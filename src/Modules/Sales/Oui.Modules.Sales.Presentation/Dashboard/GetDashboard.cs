using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Dashboard.Queries.GetDashboard;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Dashboard;

internal sealed class GetDashboard : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/dashboard", async (string? period, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetDashboardQuery(period ?? "today"), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:dashboard.view")
        .WithTags("Dashboard");
    }
}
