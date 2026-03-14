using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Commands.OpenRegister;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class OpenRegister : IEndpoint
{
    internal sealed record Request(decimal OpeningAmount);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/pos/register/open", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("user_id")
                ?? httpContext.User.FindFirstValue("sub")
                ?? "unknown";
            var userName = httpContext.User.FindFirstValue("name")
                ?? httpContext.User.FindFirstValue("display_name")
                ?? httpContext.User.FindFirstValue(ClaimTypes.Name)
                ?? httpContext.User.FindFirstValue(ClaimTypes.Email)
                ?? "Operador";

            var result = await sender.Send(new OpenRegisterCommand(request.OpeningAmount, userId, userName), ct);

            return result.Match(
                value => Results.Created($"api/pos/register/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.register.open")
        .WithTags("POS - Cash Register");
    }
}
