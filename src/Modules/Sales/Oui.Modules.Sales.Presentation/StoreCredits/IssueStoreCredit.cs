using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Commands.IssueStoreCredit;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class IssueStoreCredit : IEndpoint
{
    internal sealed record Request(long SupplierId, decimal Amount, DateTime? ExpiresOn, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/store-credits", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(new IssueStoreCreditCommand(
                request.SupplierId, request.Amount, request.ExpiresOn, request.Notes, userEmail), ct);

            return result.Match(
                value => Results.Created($"api/store-credits/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Store Credits");
    }
}
