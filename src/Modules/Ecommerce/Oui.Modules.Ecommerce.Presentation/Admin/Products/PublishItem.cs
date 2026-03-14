using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishItem;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class PublishItem : IEndpoint
{
    internal sealed record Request(Guid ItemExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/ecommerce/admin/publish", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new PublishItemCommand(request.ItemExternalId), ct);

            return result.Match(
                value => Results.Created($"api/ecommerce/admin/products/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.publish")
        .WithTags("Ecommerce Admin");
    }
}
