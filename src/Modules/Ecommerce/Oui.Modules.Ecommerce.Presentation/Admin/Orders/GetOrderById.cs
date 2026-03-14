using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrderById;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Orders;

internal sealed class GetOrderById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ecommerce/admin/orders/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetOrderByIdQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.orders.view")
        .WithTags("Ecommerce Admin");
    }
}
