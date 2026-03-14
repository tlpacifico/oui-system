using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Settlements.Queries.GetSettlements;
using shs.Application.Presentation;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Presentation.Settlements;

internal sealed class GetSettlements : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/settlements", async (long? supplierId, SettlementStatus? status, int? page, int? pageSize, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSettlementsQuery(supplierId, status, page ?? 1, pageSize ?? 20), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Settlements");
    }
}
