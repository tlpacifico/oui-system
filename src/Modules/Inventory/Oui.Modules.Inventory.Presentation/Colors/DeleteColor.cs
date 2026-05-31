using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Colors.Commands.DeleteColor;

namespace Oui.Modules.Inventory.Presentation.Colors;

internal sealed class DeleteColor : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/colors/{externalId:guid}", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteColorCommand(externalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.colors.manage")
        .WithTags("Colors");
    }
}
