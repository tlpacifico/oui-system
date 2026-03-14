using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Roles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Roles;

internal sealed class DeleteRoleEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/roles/{externalId:guid}", async (
            Guid externalId,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new DeleteRoleCommand(externalId, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Role deleted successfully." }),
                ApiResults.Problem);
        })
        .WithTags("Roles")
        .RequireAuthorization("Permission:admin.roles.delete");
    }
}
