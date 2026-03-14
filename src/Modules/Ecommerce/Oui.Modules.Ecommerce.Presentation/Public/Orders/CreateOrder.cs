using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Ecommerce.Application.Public.Orders.Commands.CreateOrder;

namespace Oui.Modules.Ecommerce.Presentation.Public.Orders;

internal sealed class CreateOrder : IEndpoint
{
    internal sealed record Request(
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        List<string> ProductSlugs,
        string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/store/orders", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateOrderCommand(
                request.CustomerName,
                request.CustomerEmail,
                request.CustomerPhone,
                request.ProductSlugs,
                request.Notes), ct);

            return result.Match(
                value => Results.Created($"api/store/orders/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Store");
    }
}
