using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.RolePermissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.RolePermissions;

internal sealed class AssignPermissionToRoleEndpoint : IEndpoint
{
    internal sealed record Request(Guid PermissionExternalId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/roles/{roleExternalId:guid}/permissions", async (
            Guid roleExternalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new AssignPermissionToRoleCommand(
                roleExternalId, request.PermissionExternalId, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Permission assigned successfully." }),
                ApiResults.Problem);
        })
        .WithTags("RolePermissions")
        .RequireAuthorization("Permission:admin.roles.manage-permissions");
    }
}
