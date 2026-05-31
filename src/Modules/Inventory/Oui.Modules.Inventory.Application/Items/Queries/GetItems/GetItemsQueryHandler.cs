using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Queries.GetItems;

internal sealed class GetItemsQueryHandler(InventoryDbContext db, EcommerceDbContext ecommerceDb)
    : IQueryHandler<GetItemsQuery, PagedResult<ItemListItemResponse>>
{
    public async Task<Result<PagedResult<ItemListItemResponse>>> Handle(
        GetItemsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Items
            .Include(i => i.Brand)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(s) ||
                i.IdentificationNumber.ToLower().Contains(s));
        }

        if (request.BrandExternalId.HasValue)
            query = query.Where(i => i.Brand.ExternalId == request.BrandExternalId.Value);

        if (request.CategoryExternalId.HasValue)
            query = query.Where(i => i.CategoryId != null && i.Category!.ExternalId == request.CategoryExternalId.Value);

        if (request.SupplierExternalId.HasValue)
            query = query.Where(i => i.SupplierId != null && i.Supplier!.ExternalId == request.SupplierExternalId.Value);

        if (request.ColorExternalId.HasValue)
            query = query.Where(i => i.Colors.Any(c => c.ExternalId == request.ColorExternalId.Value));

        if (!string.IsNullOrWhiteSpace(request.Size))
            query = query.Where(i => i.Size == request.Size);

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ItemStatus>(request.Status, out var itemStatus))
            query = query.Where(i => i.Status == itemStatus);

        if (!string.IsNullOrWhiteSpace(request.Condition) && Enum.TryParse<ItemCondition>(request.Condition, out var itemCondition))
            query = query.Where(i => i.Condition == itemCondition);

        if (!string.IsNullOrWhiteSpace(request.AcquisitionType) && Enum.TryParse<AcquisitionType>(request.AcquisitionType, out var acquisitionType))
            query = query.Where(i => i.AcquisitionType == acquisitionType);

        if (request.MinPrice.HasValue)
            query = query.Where(i => i.EvaluatedPrice >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(i => i.EvaluatedPrice <= request.MaxPrice.Value);

        if (request.CreatedFrom.HasValue)
            query = query.Where(i => i.CreatedOn >= request.CreatedFrom.Value.Date);

        if (request.CreatedTo.HasValue)
            query = query.Where(i => i.CreatedOn < request.CreatedTo.Value.Date.AddDays(1));

        query = (request.SortBy?.ToLower(), request.SortDir?.ToLower()) switch
        {
            ("name", "asc") => query.OrderBy(i => i.Name),
            ("name", _) => query.OrderByDescending(i => i.Name),
            ("price", "asc") => query.OrderBy(i => i.EvaluatedPrice),
            ("price", _) => query.OrderByDescending(i => i.EvaluatedPrice),
            ("days", "asc") => query.OrderBy(i => i.DaysInStock),
            ("days", _) => query.OrderByDescending(i => i.DaysInStock),
            ("date", "asc") => query.OrderBy(i => i.CreatedOn),
            _ => query.OrderByDescending(i => i.CreatedOn)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var rawItems = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(i => new
            {
                i.Id,
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                BrandName = i.Brand.Name,
                i.Size,
                i.Color,
                i.EvaluatedPrice,
                Status = i.Status.ToString(),
                PrimaryPhotoUrl = i.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.ThumbnailPath ?? p.FilePath).FirstOrDefault(),
                i.DaysInStock,
                i.CreatedOn
            })
            .ToListAsync(cancellationToken);

        var itemIds = rawItems.Select(i => i.Id).ToList();
        var ecommerceInfo = await ecommerceDb.EcommerceProducts
            .Where(ep => itemIds.Contains(ep.ItemId) && ep.Status != EcommerceProductStatus.Unpublished && ep.Status != EcommerceProductStatus.Sold)
            .Select(ep => new { ep.ItemId, ep.ExternalId, ep.Slug, Status = ep.Status.ToString() })
            .ToListAsync(cancellationToken);

        var ecommerceLookup = ecommerceInfo.ToDictionary(e => e.ItemId);

        var items = rawItems.Select(i =>
        {
            ecommerceLookup.TryGetValue(i.Id, out var ec);
            return new ItemListItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.BrandName,
                i.Size,
                i.Color,
                i.EvaluatedPrice,
                i.Status,
                i.PrimaryPhotoUrl,
                i.DaysInStock,
                i.CreatedOn,
                ec != null ? (Guid?)ec.ExternalId : null,
                ec?.Slug,
                ec?.Status);
        }).ToList();

        return new PagedResult<ItemListItemResponse>(
            items,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
    }
}
