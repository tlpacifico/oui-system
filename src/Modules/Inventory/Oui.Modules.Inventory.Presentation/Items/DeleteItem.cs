using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.DeleteItem;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class DeleteItem : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/inventory/items/{externalId:guid}", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteItemCommand(externalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.delete")
        .WithTags("Inventory");
    }
}
