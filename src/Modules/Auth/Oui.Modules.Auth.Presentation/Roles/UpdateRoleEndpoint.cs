using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Roles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Roles;

internal sealed class UpdateRoleEndpoint : IEndpoint
{
    internal sealed record Request(string Name, string? Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/roles/{externalId:guid}", async (
            Guid externalId,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            var command = new UpdateRoleCommand(externalId, request.Name, request.Description, email);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Role updated successfully." }),
                ApiResults.Problem);
        })
        .WithTags("Roles")
        .RequireAuthorization("Permission:admin.roles.update");
    }
}
