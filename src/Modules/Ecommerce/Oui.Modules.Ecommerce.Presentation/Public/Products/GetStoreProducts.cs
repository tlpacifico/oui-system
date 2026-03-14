using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetStoreProducts;

namespace Oui.Modules.Ecommerce.Presentation.Public.Products;

internal sealed class GetStoreProducts : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store/products", async (
            string? search,
            string? brand,
            string? category,
            string? size,
            string? color,
            string? condition,
            decimal? minPrice,
            decimal? maxPrice,
            string? sort,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStoreProductsQuery(
                search,
                brand,
                category,
                size,
                color,
                condition,
                minPrice,
                maxPrice,
                sort,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Store");
    }
}
