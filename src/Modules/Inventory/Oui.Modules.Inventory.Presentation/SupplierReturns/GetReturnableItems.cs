using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturnableItems;

namespace Oui.Modules.Inventory.Presentation.SupplierReturns;

internal sealed class GetReturnableItems : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/returns/returnable-items", async (Guid supplierExternalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReturnableItemsQuery(supplierExternalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.returns.manage")
        .WithTags("SupplierReturns");
    }
}
