using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.RolePermissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.RolePermissions;

internal sealed class RevokePermissionFromRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/roles/{roleExternalId:guid}/permissions/{permissionExternalId:guid}", async (
            Guid roleExternalId,
            Guid permissionExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RevokePermissionFromRoleCommand(roleExternalId, permissionExternalId);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Permission revoked successfully." }),
                ApiResults.Problem);
        })
        .WithTags("RolePermissions")
        .RequireAuthorization("Permission:admin.roles.manage-permissions");
    }
}
