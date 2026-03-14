using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Queries.GetItemById;

internal sealed class GetItemByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetItemByIdQuery, ItemDetailResponse>
{
    public async Task<Result<ItemDetailResponse>> Handle(
        GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .Include(i => i.Tags)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(i => i.ExternalId == request.ExternalId, cancellationToken);

        if (item is null)
            return Result.Failure<ItemDetailResponse>(ItemErrors.NotFound);

        return new ItemDetailResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Description,
            new BrandInfo(item.Brand.Id, item.Brand.Name),
            item.Category != null ? new CategoryInfo(item.Category.Id, item.Category.Name) : null,
            item.Size,
            item.Color,
            item.Composition,
            item.Condition.ToString(),
            item.EvaluatedPrice,
            item.CostPrice,
            item.FinalSalePrice,
            item.Status.ToString(),
            item.AcquisitionType.ToString(),
            item.Origin.ToString(),
            item.Supplier != null ? new SupplierInfo(item.Supplier.Id, item.Supplier.Name) : null,
            item.CommissionPercentage,
            item.CommissionAmount,
            item.IsRejected,
            item.RejectionReason,
            item.SoldAt,
            item.DaysInStock,
            item.Tags.Select(t => new TagInfo(t.Id, t.Name, t.Color)).ToList(),
            item.Photos.Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary)).ToList(),
            item.CreatedOn,
            item.CreatedBy,
            item.UpdatedOn,
            item.UpdatedBy);
    }
}
