using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptionReceipt;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class GetReceptionReceipt : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/receptions/{externalId:guid}/receipt", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReceptionReceiptQuery(externalId), ct);
            return result.Match(
                html => Results.Content(html, "text/html; charset=utf-8"),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.view")
        .WithTags("Consignment");
    }
}
