using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Auth;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Auth;

internal sealed class LoginEndpoint : IEndpoint
{
    internal sealed record Request(string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await sender.Send(command, ct);

            return result.Match(
                response => Results.Ok(response),
                ApiResults.Problem);
        })
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
