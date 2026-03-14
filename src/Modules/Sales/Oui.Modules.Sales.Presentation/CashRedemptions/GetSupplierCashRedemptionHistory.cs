using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.CashRedemptions.Queries.GetSupplierCashRedemptionHistory;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.CashRedemptions;

internal sealed class GetSupplierCashRedemptionHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/cash-redemptions/supplier/{supplierId:long}/history", async (long supplierId, int? page, int? pageSize, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetSupplierCashRedemptionHistoryQuery(supplierId, page ?? 1, pageSize ?? 20), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Cash Redemptions");
    }
}
