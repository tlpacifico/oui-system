using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.UserRoles;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.UserRoles;

internal sealed class GetUserRolesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{userExternalId:guid}/roles", async (
            Guid userExternalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetUserRolesQuery(userExternalId);
            var result = await sender.Send(query, ct);

            return result.Match(
                roles => Results.Ok(roles),
                ApiResults.Problem);
        })
        .WithTags("UserRoles")
        .RequireAuthorization("Permission:admin.users.view");
    }
}
