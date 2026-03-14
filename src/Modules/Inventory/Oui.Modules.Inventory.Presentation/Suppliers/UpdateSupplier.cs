using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Suppliers.Commands.UpdateSupplier;

namespace Oui.Modules.Inventory.Presentation.Suppliers;

internal sealed class UpdateSupplier : IEndpoint
{
    internal sealed record Request(
        string Name,
        string Email,
        string PhoneNumber,
        string? TaxNumber,
        string Initial,
        string? Notes,
        decimal? CreditPercentageInStore,
        decimal? CashRedemptionPercentage);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/suppliers/{externalId:guid}", async (Guid externalId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateSupplierCommand(
                externalId,
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.TaxNumber,
                request.Initial,
                request.Notes,
                request.CreditPercentageInStore,
                request.CashRedemptionPercentage), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.suppliers.manage")
        .WithTags("Suppliers");
    }
}
