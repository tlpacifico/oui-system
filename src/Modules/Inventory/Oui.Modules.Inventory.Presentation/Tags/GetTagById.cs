using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Tags.Queries.GetTagById;

namespace Oui.Modules.Inventory.Presentation.Tags;

internal sealed class GetTagById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/tags/{externalId:guid}", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetTagByIdQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.tags.manage")
        .WithTags("Tags");
    }
}
