using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Queries.GetSupplierStoreCredits;
using shs.Application.Presentation;
using shs.Domain.Entities;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class GetSupplierStoreCredits : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store-credits/supplier/{supplierId:long}", async (long supplierId, StoreCreditStatus? status, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSupplierStoreCreditsQuery(supplierId, status), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Store Credits");
    }
}
