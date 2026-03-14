using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.DeleteProductPhoto;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class DeleteProductPhoto : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/ecommerce/admin/products/{externalId:guid}/photos/{photoExternalId:guid}", async (
            Guid externalId,
            Guid photoExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteProductPhotoCommand(externalId, photoExternalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.update")
        .WithTags("Ecommerce Admin");
    }
}
