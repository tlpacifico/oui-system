using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Commands.UseStoreCredit;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class UseStoreCredit : IEndpoint
{
    internal sealed record Request(decimal Amount, long? SaleId, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/store-credits/{externalId:guid}/use", async (Guid externalId, Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new UseStoreCreditCommand(
                externalId, request.Amount, request.SaleId, request.Notes, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.create")
        .WithTags("Store Credits");
    }
}
