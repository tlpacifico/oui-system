using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetBrands;

namespace Oui.Modules.Ecommerce.Presentation.Public.Products;

internal sealed class GetBrands : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store/brands", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetBrandsQuery(), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Store");
    }
}
