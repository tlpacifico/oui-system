using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Auth.Application.Permissions;
using shs.Application.Presentation;

namespace Oui.Modules.Auth.Presentation.Permissions;

internal sealed class GetPermissionCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/permissions/categories", async (
            ISender sender,
            CancellationToken ct) =>
        {
            var query = new GetPermissionCategoriesQuery();
            var result = await sender.Send(query, ct);

            return result.Match(
                categories => Results.Ok(categories),
                ApiResults.Problem);
        })
        .WithTags("Permissions")
        .RequireAuthorization("Permission:admin.permissions.view");
    }
}
