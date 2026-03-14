using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.ReorderPhotos;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class ReorderPhotos : IEndpoint
{
    internal sealed record Request(Guid[] PhotoExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/inventory/items/{externalId:guid}/photos/reorder", async (
            Guid externalId,
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ReorderPhotosCommand(externalId, request.PhotoExternalIds), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.update")
        .WithTags("Inventory");
    }
}
