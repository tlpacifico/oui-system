using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Commands.CancelStoreCredit;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class CancelStoreCredit : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/store-credits/{externalId:guid}", async (Guid externalId, string? reason, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new CancelStoreCreditCommand(externalId, reason, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Store Credits");
    }
}
