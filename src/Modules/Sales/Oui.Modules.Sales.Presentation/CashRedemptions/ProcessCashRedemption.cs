using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.CashRedemptions.Commands.ProcessCashRedemption;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.CashRedemptions;

internal sealed class ProcessCashRedemption : IEndpoint
{
    internal sealed record Request(long SupplierId, decimal Amount, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/cash-redemptions", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new ProcessCashRedemptionCommand(
                request.SupplierId, request.Amount, request.Notes, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Cash Redemptions");
    }
}
