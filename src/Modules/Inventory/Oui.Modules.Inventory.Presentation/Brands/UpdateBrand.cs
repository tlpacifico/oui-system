using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Brands.Commands.UpdateBrand;

namespace Oui.Modules.Inventory.Presentation.Brands;

internal sealed class UpdateBrand : IEndpoint
{
    internal sealed record Request(string Name, string? Description, string? LogoUrl);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/brands/{externalId:guid}", async (Guid externalId, Request request, HttpContext httpContext, ISender sender, CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;

            var result = await sender.Send(new UpdateBrandCommand(
                externalId,
                request.Name,
                request.Description,
                request.LogoUrl,
                userEmail), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.brands.manage")
        .WithTags("Brands");
    }
}
