using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Users;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Users;

internal sealed class DeleteUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{externalId:guid}", async (
            Guid externalId,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new DeleteUserCommand(externalId, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.NoContent(),
                ApiResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization("Permission:admin.users.delete");
    }
}
