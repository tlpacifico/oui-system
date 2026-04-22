using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Users;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Users;

internal sealed class CreateUserEndpoint : IEndpoint
{
    internal sealed record Request(string Email, string Password, string? DisplayName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users", async (
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new CreateUserCommand(request.Email, request.Password, request.DisplayName, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                externalId => Results.Created($"/api/users/{externalId}", new { externalId }),
                ApiResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization("Permission:admin.users.create");
    }
}
