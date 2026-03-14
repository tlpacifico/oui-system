using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Permissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Permissions;

internal sealed class DeletePermissionEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/permissions/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new DeletePermissionCommand(externalId);
            var result = await sender.Send(command, ct);

            return result.Match(
                () => Results.Ok(new { message = "Permissao eliminada com sucesso." }),
                ApiResults.Problem);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Permission:admin.permissions.delete");
    }
}
