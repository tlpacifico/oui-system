using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Colors.Queries.GetAllColors;

namespace Oui.Modules.Inventory.Presentation.Colors;

internal sealed class GetAllColors : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/colors", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllColorsQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.colors.manage")
        .WithTags("Colors");
    }
}
