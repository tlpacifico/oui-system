using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Commands.CreateConsignmentItem;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class CreateConsignmentItem : IEndpoint
{
    internal sealed record Request(
        Guid ReceptionExternalId,
        string Name,
        string? Description,
        long BrandId,
        long? CategoryId,
        string Size,
        string Color,
        string? Composition,
        string Condition,
        decimal EvaluatedPrice,
        long[] TagIds,
        bool IsRejected,
        string? RejectionReason);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/inventory/items/consignment", async (Request request, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateConsignmentItemCommand(
                request.ReceptionExternalId,
                request.Name,
                request.Description,
                request.BrandId,
                request.CategoryId,
                request.Size,
                request.Color,
                request.Composition,
                request.Condition,
                request.EvaluatedPrice,
                request.TagIds,
                request.IsRejected,
                request.RejectionReason), ct);

            return result.Match(
                value => Results.Created($"api/inventory/items/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.create")
        .WithTags("Inventory");
    }
}
