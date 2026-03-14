using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Users;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Users;

internal sealed class GetUserByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{externalId:guid}", async (
            Guid externalId,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetUserByIdQuery(externalId);
            var result = await sender.Send(query, ct);

            return result.Match(
                user => Results.Ok(user),
                ApiResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization("Permission:admin.users.view");
    }
}
