using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetProductBySlug;

namespace Oui.Modules.Ecommerce.Presentation.Public.Products;

internal sealed class GetProductBySlug : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store/products/{slug}", async (
            string slug,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductBySlugQuery(slug), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Store");
    }
}
