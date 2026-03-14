using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Admin.Orders.Commands.CancelOrder;

namespace Oui.Modules.Ecommerce.Presentation.Admin.Orders;

internal sealed class CancelOrder : IEndpoint
{
    internal sealed record Request(string? Reason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/ecommerce/admin/orders/{externalId:guid}/cancel", async (
            Guid externalId,
            Request? request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new CancelOrderCommand(externalId, request?.Reason), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:ecommerce.orders.manage")
        .WithTags("Ecommerce Admin");
    }
}
