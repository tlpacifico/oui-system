using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.RolePermissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.RolePermissions;

internal sealed class AssignBulkPermissionsToRoleEndpoint : IEndpoint
{
    internal sealed record Request(List<Guid> PermissionExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/roles/{roleExternalId:guid}/permissions/bulk", async (
            Guid roleExternalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new AssignBulkPermissionsToRoleCommand(
                roleExternalId, request.PermissionExternalIds, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                count => Results.Ok(new { message = $"{count} permission(s) assigned successfully." }),
                ApiResults.Problem);
        })
        .WithTags("RolePermissions")
        .RequireAuthorization("Permission:admin.roles.manage-permissions");
    }
}
