using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Approvals.Queries.GetApprovalDetails;

namespace Oui.Modules.Inventory.Presentation.Approvals;

internal sealed class GetApprovalDetails : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/approval/{token}", async (string token, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetApprovalDetailsQuery(token), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags("Approvals");
    }
}
