using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Categories.Queries.GetAllCategories;

namespace Oui.Modules.Inventory.Presentation.Categories;

internal sealed class GetAllCategories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/categories", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllCategoriesQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.categories.manage")
        .WithTags("Categories");
    }
}
