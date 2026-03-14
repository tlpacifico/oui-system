using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Suppliers.Commands.DeleteSupplier;

namespace Oui.Modules.Inventory.Presentation.Suppliers;

internal sealed class DeleteSupplier : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("api/suppliers/{externalId:guid}", async (Guid externalId, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new DeleteSupplierCommand(externalId), ct);
            return result.Match(() => Results.NoContent(), ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.suppliers.manage")
        .WithTags("Suppliers");
    }
}
