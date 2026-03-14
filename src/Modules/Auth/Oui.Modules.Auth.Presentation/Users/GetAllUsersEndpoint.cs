using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Users;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Users;

internal sealed class GetAllUsersEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users", async (
            string? search,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAllUsersQuery(search);
            var result = await sender.Send(query, ct);

            return result.Match(
                users => Results.Ok(users),
                ApiResults.Problem);
        })
        .WithTags("Users")
        .RequireAuthorization("Permission:admin.users.view");
    }
}
