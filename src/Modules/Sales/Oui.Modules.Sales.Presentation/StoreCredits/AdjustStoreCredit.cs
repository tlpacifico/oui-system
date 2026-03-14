using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Commands.AdjustStoreCredit;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class AdjustStoreCredit : IEndpoint
{
    internal sealed record Request(decimal AdjustmentAmount, string? Reason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/store-credits/{externalId:guid}/adjust", async (Guid externalId, Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new AdjustStoreCreditCommand(
                externalId, request.AdjustmentAmount, request.Reason, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Store Credits");
    }
}
