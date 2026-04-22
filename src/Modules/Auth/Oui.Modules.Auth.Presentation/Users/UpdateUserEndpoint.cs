using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Users;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Users;

internal sealed class UpdateUserEndpoint : IEndpoint
{
    internal sealed record Request(string? DisplayName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users/{externalId:guid}", async (
            Guid externalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new UpdateUserCommand(externalId, request.DisplayName, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Utilizador atualizado com sucesso." }),
                ApiResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization("Permission:admin.users.update");
    }
}
