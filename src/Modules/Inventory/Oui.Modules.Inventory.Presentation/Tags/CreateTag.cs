using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Tags.Commands.CreateTag;

namespace Oui.Modules.Inventory.Presentation.Tags;

internal sealed class CreateTag : IEndpoint
{
    internal sealed record Request(string Name, string? Color);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/tags", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateTagCommand(
                request.Name,
                request.Color), ct);

            return result.Match(
                value => Results.Created($"api/tags/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.tags.manage")
        .WithTags("Tags");
    }
}
