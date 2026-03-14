using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Categories.Commands.CreateCategory;

namespace Oui.Modules.Inventory.Presentation.Categories;

internal sealed class CreateCategory : IEndpoint
{
    internal sealed record Request(string Name, string? Description, Guid? ParentCategoryExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateCategoryCommand(
                request.Name,
                request.Description,
                request.ParentCategoryExternalId), ct);

            return result.Match(
                value => Results.Created($"api/categories/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.categories.manage")
        .WithTags("Categories");
    }
}
