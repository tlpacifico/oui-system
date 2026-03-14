using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Pos.Queries.GetPosSuppliers;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Pos;

internal sealed class GetPosSuppliers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/pos/suppliers", async (string? search, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetPosSuppliersQuery(search), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.create")
        .WithTags("POS");
    }
}
