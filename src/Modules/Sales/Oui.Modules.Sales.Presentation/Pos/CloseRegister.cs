using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Commands.CloseRegister;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class CloseRegister : IEndpoint
{
    internal sealed record Request(Guid RegisterExternalId, decimal ClosingAmount, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/pos/register/close", async (Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.User.FindFirstValue("user_id")
                ?? httpContext.User.FindFirstValue("sub")
                ?? "unknown";

            var result = await sender.Send(new CloseRegisterCommand(
                request.RegisterExternalId, request.ClosingAmount, request.Notes, userId), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.register.close")
        .WithTags("POS - Cash Register");
    }
}
