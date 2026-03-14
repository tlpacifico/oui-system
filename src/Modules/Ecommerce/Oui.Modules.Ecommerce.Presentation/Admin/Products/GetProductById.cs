using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Queries.GetProductById;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class GetProductById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ecommerce/admin/products/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetProductByIdQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.view")
        .WithTags("Ecommerce Admin");
    }
}
