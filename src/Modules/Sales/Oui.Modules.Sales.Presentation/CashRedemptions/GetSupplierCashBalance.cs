using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashBalance;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.CashRedemptions;

internal sealed class GetSupplierCashBalance : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/cash-redemptions/supplier/{supplierId:long}/balance", async (long supplierId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSupplierCashBalanceQuery(supplierId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Cash Redemptions");
    }
}
