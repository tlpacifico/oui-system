using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCreditBalance;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class GetSupplierStoreCreditBalance : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store-credits/supplier/{supplierId:long}/balance", async (long supplierId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSupplierStoreCreditBalanceQuery(supplierId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.create")
        .WithTags("Store Credits");
    }
}
