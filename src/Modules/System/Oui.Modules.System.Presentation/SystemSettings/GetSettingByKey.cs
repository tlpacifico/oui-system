using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.System.Application.SystemSettings.Queries.GetAllSettings;
using Oui.Modules.System.Application.SystemSettings.Queries.GetSettingByKey;

namespace Oui.Modules.System.Presentation.SystemSettings;

internal sealed class GetSettingByKey : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/system-settings/{key}", async (string key, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSettingByKeyQuery(key), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:admin.settings.view")
        .WithTags("System Settings");
    }
}
