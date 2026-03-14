using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Me;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Me;

internal sealed class GetMyPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/me/permissions", async (
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
                return Results.Unauthorized();

            var query = new GetMyPermissionsQuery(email);
            var result = await sender.Send(query, ct);

            return result.Match(
                permissions => Results.Ok(permissions),
                ApiResults.Problem);
        })
        .WithTags("Me")
        .RequireAuthorization();
    }
}
