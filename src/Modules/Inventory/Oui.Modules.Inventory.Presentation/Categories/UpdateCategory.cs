using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Categories.Commands.UpdateCategory;

namespace Oui.Modules.Inventory.Presentation.Categories;

internal sealed class UpdateCategory : IEndpoint
{
    internal sealed record Request(string Name, string? Description, Guid? ParentCategoryExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/categories/{externalId:guid}", async (Guid externalId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateCategoryCommand(
                externalId,
                request.Name,
                request.Description,
                request.ParentCategoryExternalId), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.categories.manage")
        .WithTags("Categories");
    }
}
