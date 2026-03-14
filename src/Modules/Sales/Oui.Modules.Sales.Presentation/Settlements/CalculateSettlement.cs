using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Settlements.Queries.CalculateSettlement;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Settlements;

internal sealed class CalculateSettlement : IEndpoint
{
    internal sealed record Request(long SupplierId, DateTime PeriodStart, DateTime PeriodEnd);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/settlements/calculate", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CalculateSettlementQuery(
                request.SupplierId, request.PeriodStart, request.PeriodEnd), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Settlements");
    }
}
