using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Colors.Commands.CreateColor;

namespace Oui.Modules.Inventory.Presentation.Colors;

internal sealed class CreateColor : IEndpoint
{
    internal sealed record Request(string Name, string? HexCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/colors", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateColorCommand(
                request.Name,
                request.HexCode), ct);

            return result.Match(
                value => Results.Created($"api/colors/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.colors.manage")
        .WithTags("Colors");
    }
}
