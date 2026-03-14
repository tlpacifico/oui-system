using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Approvals.Commands.ApproveByToken;

namespace Oui.Modules.Inventory.Presentation.Approvals;

internal sealed class ApproveByToken : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/approval/{token}/approve", async (string token, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ApproveByTokenCommand(token), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Approvals");
    }
}
