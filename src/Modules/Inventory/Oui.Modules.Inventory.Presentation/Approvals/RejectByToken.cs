using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Approvals.Commands.RejectByToken;

namespace Oui.Modules.Inventory.Presentation.Approvals;

internal sealed class RejectByToken : IEndpoint
{
    internal sealed record Request(string? Message);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/approval/{token}/reject", async (string token, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new RejectByTokenCommand(token, request.Message), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Approvals");
    }
}
