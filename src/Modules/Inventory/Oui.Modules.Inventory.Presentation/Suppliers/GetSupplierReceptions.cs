using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Suppliers.Queries.GetSupplierReceptions;

namespace Oui.Modules.Inventory.Presentation.Suppliers;

internal sealed class GetSupplierReceptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/suppliers/{externalId:guid}/receptions", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSupplierReceptionsQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.suppliers.manage")
        .WithTags("Suppliers");
    }
}
