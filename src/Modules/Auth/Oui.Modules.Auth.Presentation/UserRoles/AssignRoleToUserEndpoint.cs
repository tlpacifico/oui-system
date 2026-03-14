using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.UserRoles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.UserRoles;

internal sealed class AssignRoleToUserEndpoint : IEndpoint
{
    internal sealed record Request(Guid RoleExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/{userExternalId:guid}/roles", async (
            Guid userExternalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new AssignRoleToUserCommand(
                userExternalId, request.RoleExternalId, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Role assigned successfully." }),
                ApiResults.Problem);
        })
        .WithTags("UserRoles")
        .RequireAuthorization("Permission:admin.users.manage-roles");
    }
}
