using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Queries.GetCurrentRegister;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class GetCurrentRegister : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/pos/register/current", async (HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("user_id")
                ?? httpContext.User.FindFirstValue("sub")
                ?? "unknown";

            var result = await sender.Send(new GetCurrentRegisterQuery(userId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.register.view")
        .WithTags("POS - Cash Register");
    }
}
