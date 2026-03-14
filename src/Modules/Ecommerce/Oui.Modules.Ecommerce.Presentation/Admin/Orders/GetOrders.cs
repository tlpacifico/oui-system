using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Orders.Queries.GetOrders;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Orders;

internal sealed class GetOrders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/ecommerce/admin/orders", async (
            string? status,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetOrdersQuery(
                status,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.orders.view")
        .WithTags("Ecommerce Admin");
    }
}
