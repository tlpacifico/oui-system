using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Oui.Modules.Sales.Application.StoreCredits.Queries.GetStoreCreditTransactions;
using shs.Application.Presentation;

namespace Oui.Modules.Sales.Presentation.StoreCredits;

internal sealed class GetStoreCreditTransactions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/store-credits/{externalId:guid}/transactions", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetStoreCreditTransactionsQuery(externalId), ct);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:reports.view")
        .WithTags("Store Credits");
    }
}
