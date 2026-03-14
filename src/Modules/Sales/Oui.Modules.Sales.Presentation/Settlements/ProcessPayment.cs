using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Settlements.Commands.ProcessPayment;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Settlements;

internal sealed class ProcessPayment : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/settlements/{externalId:guid}/process-payment", async (Guid externalId, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new ProcessPaymentCommand(externalId, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Settlements");
    }
}
