using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UnpublishProduct;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class UnpublishProduct : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/ecommerce/admin/products/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UnpublishProductCommand(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.unpublish")
        .WithTags("Ecommerce Admin");
    }
}
