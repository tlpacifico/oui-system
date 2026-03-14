using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Roles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Roles;

internal sealed class GetAllRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/roles", async (
            string? search,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAllRolesQuery(search);
            var result = await sender.Send(query, ct);

            return result.Match(
                roles => Results.Ok(roles),
                ApiResults.Problem);
        })
        .WithTags("Roles")
        .RequireAuthorization("Permission:admin.roles.view");
    }
}
