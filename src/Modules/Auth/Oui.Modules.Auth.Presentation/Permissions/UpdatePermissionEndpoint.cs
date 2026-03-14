using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Permissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Permissions;

internal sealed class UpdatePermissionEndpoint : IEndpoint
{
    internal sealed record Request(string Name, string? Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/permissions/{externalId:guid}", async (
            Guid externalId,
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new UpdatePermissionCommand(externalId, request.Name, request.Description);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Permissao atualizada com sucesso." }),
                ApiResults.Problem);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Permission:admin.permissions.update");
    }
}
