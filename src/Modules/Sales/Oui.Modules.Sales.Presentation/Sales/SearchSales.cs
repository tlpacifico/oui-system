using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.Sales.Queries.SearchSales;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.Sales;

internal sealed class SearchSales : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/pos/sales", async (DateTime? dateFrom, DateTime? dateTo, string? search, int? page, int? pageSize, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new SearchSalesQuery(dateFrom, dateTo, search, page ?? 1, pageSize ?? 20), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:pos.sales.view")
        .WithTags("POS - Sales");
    }
}
