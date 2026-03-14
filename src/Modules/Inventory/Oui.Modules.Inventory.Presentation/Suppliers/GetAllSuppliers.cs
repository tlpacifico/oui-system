using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Suppliers.Queries.GetAllSuppliers;

namespace Oui.Modules.Inventory.Presentation.Suppliers;

internal sealed class GetAllSuppliers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/suppliers", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetAllSuppliersQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.suppliers.manage")
        .WithTags("Suppliers");
    }
}
