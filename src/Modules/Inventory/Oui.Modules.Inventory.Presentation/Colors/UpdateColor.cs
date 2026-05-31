using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Colors.Commands.UpdateColor;

namespace Oui.Modules.Inventory.Presentation.Colors;

internal sealed class UpdateColor : IEndpoint
{
    internal sealed record Request(string Name, string? HexCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/colors/{externalId:guid}", async (Guid externalId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateColorCommand(
                externalId,
                request.Name,
                request.HexCode), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.colors.manage")
        .WithTags("Colors");
    }
}
