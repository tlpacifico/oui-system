using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.UploadPhotos;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class UploadPhotos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/inventory/items/{externalId:guid}/photos", async (
            Guid externalId,
            IFormFileCollection files,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UploadPhotosCommand(externalId, files), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.update")
        .DisableAntiforgery()
        .WithTags("Inventory");
    }
}
