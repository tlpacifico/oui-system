using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Approvals.Commands.StaffApprove;

namespace Oui.Modules.Inventory.Presentation.Approvals;

internal sealed class StaffApprove : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/consignments/receptions/{externalId:guid}/approve", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new StaffApproveCommand(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.approve")
        .WithTags("Approvals");
    }
}
