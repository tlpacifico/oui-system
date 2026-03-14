using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.UserRoles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.UserRoles;

internal sealed class RevokeRoleFromUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/users/{userExternalId:guid}/roles/{roleExternalId:guid}", async (
            Guid userExternalId,
            Guid roleExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RevokeRoleFromUserCommand(userExternalId, roleExternalId);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Role revoked successfully." }),
                ApiResults.Problem);
        })
        .WithTags("UserRoles")
        .RequireAuthorization("Permission:admin.users.manage-roles");
    }
}
