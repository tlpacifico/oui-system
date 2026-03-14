using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.ConfirmOrder;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Orders;

internal sealed class ConfirmOrder : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/ecommerce/admin/orders/{externalId:guid}/confirm", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new ConfirmOrderCommand(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.orders.manage")
        .WithTags("Ecommerce Admin");
    }
}
