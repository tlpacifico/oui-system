using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Tags.Commands.UpdateTag;

namespace Oui.Modules.Inventory.Presentation.Tags;

internal sealed class UpdateTag : IEndpoint
{
    internal sealed record Request(string Name, string? Color);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/tags/{externalId:guid}", async (Guid externalId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateTagCommand(
                externalId,
                request.Name,
                request.Color), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.tags.manage")
        .WithTags("Tags");
    }
}
