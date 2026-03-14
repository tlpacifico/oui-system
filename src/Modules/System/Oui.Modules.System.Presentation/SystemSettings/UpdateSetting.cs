using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.System.Application.SystemSettings.Commands.UpdateSetting;

namespace Oui.Modules.System.Presentation.SystemSettings;

internal sealed class UpdateSetting : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/system-settings/{key}", async (
            string key,
            Request request,
            HttpContext httpContext,
            ISender sender,
            CancellationToken ct) =>
        {
            var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
            var result = await sender.Send(
                new UpdateSettingCommand(key, request.Value, userEmail), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:admin.settings.update")
        .WithTags("System Settings");
    }

    internal sealed record Request(string Value);
}
