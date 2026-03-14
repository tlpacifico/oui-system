using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Commands.CreateReception;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class CreateReception : IEndpoint
{
    internal sealed record Request(Guid? SupplierExternalId, int ItemCount, string? Notes);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/receptions", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateReceptionCommand(
                request.SupplierExternalId,
                request.ItemCount,
                request.Notes), ct);

            return result.Match(
                value => Results.Created($"api/consignments/receptions/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.create")
        .WithTags("Consignment");
    }
}
