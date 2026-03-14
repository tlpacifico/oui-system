using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Settlements.Queries.GetPendingSettlementItems;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Settlements;

internal sealed class GetPendingSettlementItems : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/settlements/pending-items", async (long? supplierId, DateTime? startDate, DateTime? endDate, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPendingSettlementItemsQuery(supplierId, startDate, endDate), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Settlements");
    }
}
