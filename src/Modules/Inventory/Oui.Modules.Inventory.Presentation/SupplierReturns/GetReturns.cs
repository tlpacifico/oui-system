using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.SupplierReturns.Queries.GetReturns;

namespace Oui.Modules.Inventory.Presentation.SupplierReturns;

internal sealed class GetReturns : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/returns", async (
            Guid? supplierExternalId,
            string? search,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReturnsQuery(
                supplierExternalId,
                search,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.returns.manage")
        .WithTags("SupplierReturns");
    }
}
