using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Commands.RemoveEvaluationItem;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class RemoveEvaluationItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/consignments/receptions/{receptionExternalId:guid}/items/{itemExternalId:guid}", async (
            Guid receptionExternalId,
            Guid itemExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new RemoveEvaluationItemCommand(receptionExternalId, itemExternalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.evaluate")
        .WithTags("Consignment");
    }
}
