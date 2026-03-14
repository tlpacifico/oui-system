using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.CreateItem;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class CreateItem : IEndpoint
{
    internal sealed record Request(
        string Name,
        string? Description,
        Guid BrandExternalId,
        Guid? CategoryExternalId,
        string Size,
        string Color,
        string? Composition,
        string Condition,
        decimal EvaluatedPrice,
        decimal? CostPrice,
        string AcquisitionType,
        string Origin,
        Guid? SupplierExternalId,
        decimal? CommissionPercentage,
        Guid[]? TagExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/inventory/items", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateItemCommand(
                request.Name,
                request.Description,
                request.BrandExternalId,
                request.CategoryExternalId,
                request.Size,
                request.Color,
                request.Composition,
                request.Condition,
                request.EvaluatedPrice,
                request.CostPrice,
                request.AcquisitionType,
                request.Origin,
                request.SupplierExternalId,
                request.CommissionPercentage,
                request.TagExternalIds), ct);

            return result.Match(
                value => Results.Created($"api/inventory/items/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.create")
        .WithTags("Inventory");
    }
}
