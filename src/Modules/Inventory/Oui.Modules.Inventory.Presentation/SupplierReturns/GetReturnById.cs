using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnById;

namespace Oui.Modules.Inventory.Presentation.SupplierReturns;

internal sealed class GetReturnById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/returns/{externalId:guid}", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReturnByIdQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.returns.manage")
        .WithTags("SupplierReturns");
    }
}
