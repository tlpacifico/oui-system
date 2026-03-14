using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Roles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Roles;

internal sealed class GetRoleByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/roles/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetRoleByIdQuery(externalId);
            var result = await sender.Send(query, ct);

            return result.Match(
                role => Results.Ok(role),
                ApiResults.Problem);
        })
        .WithTags("Roles")
        .RequireAuthorization("Permission:admin.roles.view");
    }
}
