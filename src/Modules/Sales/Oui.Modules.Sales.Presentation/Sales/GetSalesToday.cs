using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Sales.Queries.GetSalesToday;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Sales;

internal sealed class GetSalesToday : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/pos/sales/today", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSalesTodayQuery(), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.view")
        .WithTags("POS - Sales");
    }
}
