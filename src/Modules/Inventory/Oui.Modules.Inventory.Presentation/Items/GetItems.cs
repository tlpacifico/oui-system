using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using shs.Application.Presentation;
using Oui.Modules.Inventory.Application.Items.Queries.GetItems;

namespace Oui.Modules.Inventory.Presentation.Items;

internal sealed class GetItems : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/inventory/items", async (
            string? search,
            Guid? brandExternalId,
            Guid? categoryExternalId,
            Guid? supplierExternalId,
            Guid? colorExternalId,
            string? size,
            string? status,
            string? condition,
            string? acquisitionType,
            decimal? minPrice,
            decimal? maxPrice,
            DateTime? createdFrom,
            DateTime? createdTo,
            string? sortBy,
            string? sortDir,
            int page,
            int pageSize,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetItemsQuery(
                search,
                brandExternalId,
                categoryExternalId,
                supplierExternalId,
                colorExternalId,
                size,
                status,
                condition,
                acquisitionType,
                minPrice,
                maxPrice,
                createdFrom,
                createdTo,
                sortBy,
                sortDir,
                page == 0 ? 1 : page,
                pageSize == 0 ? 20 : pageSize), ct);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization("Permission:inventory.items.view")
        .WithTags("Inventory");
    }
}
