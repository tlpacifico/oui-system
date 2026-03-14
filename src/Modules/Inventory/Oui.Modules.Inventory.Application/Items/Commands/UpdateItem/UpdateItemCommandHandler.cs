using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Commands.UpdateItem;

internal sealed class UpdateItemCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateItemCommand, ItemDetailResponse>
{
    public async Task<Result<ItemDetailResponse>> Handle(
        UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ExternalId, cancellationToken);

        if (item is null)
            return Result.Failure<ItemDetailResponse>(ItemErrors.NotFound);

        if (item.Status == ItemStatus.Sold)
            return Result.Failure<ItemDetailResponse>(ItemErrors.CannotEditSoldItem);

        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == request.BrandExternalId, cancellationToken);
        if (brand is null)
            return Result.Failure<ItemDetailResponse>(ItemErrors.BrandNotFound);

        long? categoryId = null;
        if (request.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == request.CategoryExternalId.Value, cancellationToken);
            if (category is null)
                return Result.Failure<ItemDetailResponse>(ItemErrors.CategoryNotFound);
            categoryId = category.Id;
        }

        if (!Enum.TryParse<ItemCondition>(request.Condition, out var condition))
            return Result.Failure<ItemDetailResponse>(ItemErrors.InvalidCondition);

        item.Name = request.Name.Trim();
        item.Description = request.Description?.Trim();
        item.BrandId = brand.Id;
        item.CategoryId = categoryId;
        item.Size = request.Size.Trim();
        item.Color = request.Color.Trim();
        item.Composition = request.Composition?.Trim();
        item.Condition = condition;
        item.EvaluatedPrice = request.EvaluatedPrice;
        item.CostPrice = request.CostPrice;
        item.CommissionPercentage = request.CommissionPercentage ?? item.CommissionPercentage;
        item.UpdatedOn = DateTime.UtcNow;
        item.UpdatedBy = "system";

        if (request.TagExternalIds is not null)
        {
            item.Tags.Clear();
            if (request.TagExternalIds.Length > 0)
            {
                var tags = await db.Tags
                    .Where(t => request.TagExternalIds.Contains(t.ExternalId))
                    .ToListAsync(cancellationToken);
                item.Tags = tags;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        await db.Entry(item).Reference(i => i.Brand).LoadAsync(cancellationToken);
        await db.Entry(item).Reference(i => i.Category).LoadAsync(cancellationToken);
        await db.Entry(item).Reference(i => i.Supplier).LoadAsync(cancellationToken);
        await db.Entry(item).Collection(i => i.Tags).LoadAsync(cancellationToken);
        await db.Entry(item).Collection(i => i.Photos).LoadAsync(cancellationToken);

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
