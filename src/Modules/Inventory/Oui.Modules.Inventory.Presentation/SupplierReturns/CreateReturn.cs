using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.SupplierReturns.Commands.CreateReturn;

namespace Oui.Modules.Inventory.Presentation.SupplierReturns;

internal sealed class CreateReturn : IEndpoint
{
    internal sealed record Request(Guid SupplierExternalId, Guid[] ItemExternalIds, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/returns", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateReturnCommand(
                request.SupplierExternalId,
                request.ItemExternalIds,
                request.Notes), ct);

            return result.Match(
                value => Results.Created($"api/consignments/returns/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.returns.manage")
        .WithTags("SupplierReturns");
    }
}
