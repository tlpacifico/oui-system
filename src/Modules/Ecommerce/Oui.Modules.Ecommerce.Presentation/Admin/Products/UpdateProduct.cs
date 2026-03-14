using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UpdateProduct;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class UpdateProduct : IEndpoint
{
    internal sealed record Request(
        string? Title,
        string? Description,
        decimal? Price,
        string? BrandName,
        string? CategoryName,
        string? Size,
        string? Color,
        string? Condition,
        string? Composition);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/ecommerce/admin/products/{externalId:guid}", async (
            Guid externalId,
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateProductCommand(
                externalId,
                request.Title,
                request.Description,
                request.Price,
                request.BrandName,
                request.CategoryName,
                request.Size,
                request.Color,
                request.Condition,
                request.Composition), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.update")
        .WithTags("Ecommerce Admin");
    }
}
