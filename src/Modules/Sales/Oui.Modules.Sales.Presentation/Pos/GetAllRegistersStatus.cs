using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Queries.GetAllRegistersStatus;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class GetAllRegistersStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/pos/register/status", async (int? days, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllRegistersStatusQuery(days ?? 7), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.register.view")
        .WithTags("POS - Cash Register");
    }
}
