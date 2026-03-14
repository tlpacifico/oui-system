using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;
using shs.Infrastructure.Services;

namespace Oui.Modules.Inventory.Application.Items.Commands.CreateItem;

internal sealed class CreateItemCommandHandler(InventoryDbContext db, IItemIdGeneratorService idGenerator)
    : ICommandHandler<CreateItemCommand, CreateConsignmentItemResponse>
{
    public async Task<Result<CreateConsignmentItemResponse>> Handle(
        CreateItemCommand request, CancellationToken cancellationToken)
    {
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == request.BrandExternalId, cancellationToken);
        if (brand is null)
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.BrandNotFound);

        long? categoryId = null;
        if (request.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == request.CategoryExternalId.Value, cancellationToken);
            if (category is null)
                return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.CategoryNotFound);
            categoryId = category.Id;
        }

        if (!Enum.TryParse<ItemCondition>(request.Condition, out var condition))
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.InvalidCondition);

        if (!Enum.TryParse<AcquisitionType>(request.AcquisitionType, out var acquisitionType))
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.InvalidAcquisitionType);

        if (!Enum.TryParse<ItemOrigin>(request.Origin, out var origin))
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.InvalidOrigin);

        long? supplierId = null;
        if (acquisitionType == AcquisitionType.Consignment)
        {
            if (!request.SupplierExternalId.HasValue)
                return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.SupplierRequiredForConsignment);

            var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.ExternalId == request.SupplierExternalId.Value, cancellationToken);
            if (supplier is null)
                return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.SupplierNotFound);
            supplierId = supplier.Id;
        }

        var itemId = await idGenerator.GenerateNextIdAsync(supplierId, cancellationToken);
        var status = acquisitionType == AcquisitionType.OwnPurchase ? ItemStatus.ToSell : ItemStatus.Evaluated;

        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BrandId = brand.Id,
            CategoryId = categoryId,
            Size = request.Size.Trim(),
            Color = request.Color.Trim(),
            Composition = request.Composition?.Trim(),
            Condition = condition,
            EvaluatedPrice = request.EvaluatedPrice,
            CostPrice = request.CostPrice,
            Status = status,
            AcquisitionType = acquisitionType,
            Origin = origin,
            SupplierId = supplierId,
            CommissionPercentage = request.CommissionPercentage ?? 50m,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        if (request.TagExternalIds is { Length: > 0 })
        {
            var tags = await db.Tags
                .Where(t => request.TagExternalIds.Contains(t.ExternalId))
                .ToListAsync(cancellationToken);
            item.Tags = tags;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(cancellationToken);

        return new CreateConsignmentItemResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Status.ToString(),
            item.CreatedOn);
    }
}
