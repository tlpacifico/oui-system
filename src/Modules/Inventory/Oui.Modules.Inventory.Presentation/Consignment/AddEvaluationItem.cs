using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Consignment.Commands.AddEvaluationItem;

namespace Oui.Modules.Inventory.Presentation.Consignment;

internal sealed class AddEvaluationItem : IEndpoint
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
        decimal? CommissionPercentage,
        bool IsRejected,
        string? RejectionReason,
        Guid[]? TagExternalIds);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/consignments/receptions/{externalId:guid}/items", async (
            Guid externalId,
            Request request,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new AddEvaluationItemCommand(
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
                request.CommissionPercentage,
                request.IsRejected,
                request.RejectionReason,
                request.TagExternalIds), ct);

            return result.Match(
                value => Results.Created($"api/consignments/receptions/{externalId}/items/{value.ExternalId}", value),
                ApiResults.Problem);
        })
        .RequireAuthorization("Permission:consignment.receptions.evaluate")
        .WithTags("Consignment");
    }
}
