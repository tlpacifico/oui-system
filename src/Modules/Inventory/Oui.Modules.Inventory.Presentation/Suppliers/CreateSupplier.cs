using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Suppliers.Commands.CreateSupplier;

namespace Oui.Modules.Inventory.Presentation.Suppliers;

internal sealed class CreateSupplier : IEndpoint
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
        app.MapPost("api/suppliers", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateSupplierCommand(
                request.Name,
                request.Email,
                request.PhoneNumber,
                request.TaxNumber,
                request.Initial,
                request.Notes,
                request.CreditPercentageInStore,
                request.CashRedemptionPercentage), ct);

            return result.Match(
                value => Results.Created($"api/suppliers/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.suppliers.manage")
        .WithTags("Suppliers");
    }
}
