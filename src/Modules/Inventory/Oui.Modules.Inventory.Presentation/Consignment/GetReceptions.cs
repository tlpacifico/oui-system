using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Queries.GetReceptions;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class GetReceptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/consignments/receptions", async (
            string? status,
            Guid? supplierExternalId,
            string? search,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReceptionsQuery(
                status,
                supplierExternalId,
                search,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.view")
        .WithTags("Consignment");
    }
}
