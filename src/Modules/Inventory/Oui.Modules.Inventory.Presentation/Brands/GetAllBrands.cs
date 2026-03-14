using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Brands.Queries.GetAllBrands;

namespace Oui.Modules.Inventory.Presentation.Brands;

internal sealed class GetAllBrands : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/brands", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllBrandsQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.brands.manage")
        .WithTags("Brands");
    }
}
