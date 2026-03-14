using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Settlements.Commands.CreateSettlement;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Settlements;

internal sealed class CreateSettlement : IEndpoint
{
    internal sealed record Request(long SupplierId, DateTime PeriodStart, DateTime PeriodEnd, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/settlements", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

            var result = await sender.Send(new CreateSettlementCommand(
                request.SupplierId, request.PeriodStart, request.PeriodEnd, request.Notes, userEmail), ct);

            return result.Match(
                value => Results.Created($"api/settlements/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Settlements");
    }
}
