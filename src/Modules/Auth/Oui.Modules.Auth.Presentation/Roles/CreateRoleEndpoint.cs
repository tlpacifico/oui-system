using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Roles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Roles;

internal sealed class CreateRoleEndpoint : IEndpoint
{
    internal sealed record Request(string Name, string? Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/roles", async (
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new CreateRoleCommand(request.Name, request.Description, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                externalId => Results.Created($"/api/roles/{externalId}", new { externalId }),
                ApiResults.Problem);
        })
        .WithTags("Roles")
        .RequireAuthorization("Permission:admin.roles.create");
    }
}
