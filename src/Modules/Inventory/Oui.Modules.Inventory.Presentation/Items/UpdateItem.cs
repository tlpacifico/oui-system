using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.UpdateItem;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class UpdateItem : IEndpoint
{
    internal sealed record Request(
        string Name,
        string? Description,
        Guid BrandExternalId,
        Guid? CategoryExternalId,
        string Size,
        string? Color,
        string? Composition,
        string Condition,
        decimal EvaluatedPrice,
        decimal? CostPrice,
        string AcquisitionType,
        string Origin,
        Guid? SupplierExternalId,
        decimal? CommissionPercentage,
        Guid[]? TagExternalIds,
        Guid[]? ColorExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/inventory/items/{externalId:guid}", async (Guid externalId, Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new UpdateItemCommand(
                externalId,
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
                request.TagExternalIds,
                request.ColorExternalIds), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.update")
        .WithTags("Inventory");
    }
}
