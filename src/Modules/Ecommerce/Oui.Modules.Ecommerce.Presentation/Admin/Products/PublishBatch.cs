using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishBatch;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Products;

internal sealed class PublishBatch : IEndpoint
{
    internal sealed record Request(List<Guid> ItemExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/ecommerce/admin/publish-batch", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new PublishBatchCommand(request.ItemExternalIds), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.products.publish")
        .WithTags("Ecommerce Admin");
    }
}
