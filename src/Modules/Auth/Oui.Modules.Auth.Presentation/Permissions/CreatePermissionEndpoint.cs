using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Permissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Permissions;

internal sealed class CreatePermissionEndpoint : IEndpoint
{
    internal sealed record Request(string Name, string? Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/permissions", async (
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new CreatePermissionCommand(request.Name, request.Description);
            var result = await sender.Send(command, ct);

            return result.Match(
                externalId => Results.Ok(new { externalId }),
                ApiResults.Problem);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Permission:admin.permissions.create");
    }
}
