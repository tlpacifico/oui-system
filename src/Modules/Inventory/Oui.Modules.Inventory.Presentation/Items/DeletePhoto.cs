using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.DeletePhoto;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class DeletePhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/inventory/items/{itemExternalId:guid}/photos/{photoExternalId:guid}", async (
            Guid itemExternalId,
            Guid photoExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeletePhotoCommand(itemExternalId, photoExternalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.update")
        .WithTags("Inventory");
    }
}
