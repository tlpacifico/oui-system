using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UploadProductPhotos;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class UploadProductPhotos : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/ecommerce/admin/products/{externalId:guid}/photos", async (
            Guid externalId,
            IFormFileCollection files,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new UploadProductPhotosCommand(externalId, files), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.update")
        .DisableAntiforgery()
        .WithTags("Ecommerce Admin");
    }
}
