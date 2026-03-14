using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.UserRoles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.UserRoles;

internal sealed class AssignBulkRolesToUserEndpoint : IEndpoint
{
    internal sealed record Request(List<Guid> RoleExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/{userExternalId:guid}/roles/bulk", async (
            Guid userExternalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new AssignBulkRolesToUserCommand(
                userExternalId, request.RoleExternalIds, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                count => Results.Ok(new { message = $"{count} role(s) assigned successfully." }),
                ApiResults.Problem);
        })
        .WithTags("UserRoles")
        .RequireAuthorization("Permission:admin.users.manage-roles");
    }
}
