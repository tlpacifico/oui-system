using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Commands.ProcessSale;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class ProcessSale : IEndpoint
{
    internal sealed record Request(
        Guid CashRegisterId,
        SaleItemInput[] Items,
        SalePaymentInput[] Payments,
        decimal? DiscountPercentage,
        string? DiscountReason,
        Guid? CustomerExternalId,
        string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/pos/sales", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("user_id")
                ?? httpContext.User.FindFirstValue("sub")
                ?? "unknown";

            var result = await sender.Send(new ProcessSaleCommand(
                request.CashRegisterId,
                request.Items,
                request.Payments,
                request.DiscountPercentage,
                request.DiscountReason,
                request.CustomerExternalId,
                request.Notes,
                userId), ct);

            return result.Match(
                value => Results.Created($"api/pos/sales/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.create")
        .WithTags("POS - Sales");
    }
}
