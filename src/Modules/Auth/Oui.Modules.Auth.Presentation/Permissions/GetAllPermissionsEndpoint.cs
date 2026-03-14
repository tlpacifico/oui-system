using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Permissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Permissions;

internal sealed class GetAllPermissionsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/permissions", async (
            string? category,
            string? search,
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetAllPermissionsQuery(category, search);
            var result = await sender.Send(query, ct);

            return result.Match(
                permissions => Results.Ok(permissions),
                ApiResults.Problem);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Permission:admin.permissions.view");
    }
}
