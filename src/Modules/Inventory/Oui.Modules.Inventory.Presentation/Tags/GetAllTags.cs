using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Tags.Queries.GetAllTags;

namespace Oui.Modules.Inventory.Presentation.Tags;

internal sealed class GetAllTags : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllTagsQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.tags.manage")
        .WithTags("Tags");
    }
}
