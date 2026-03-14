using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;
using shs.Infrastructure.Services;

namespace Oui.Modules.Inventory.Application.Items.Commands.CreateConsignmentItem;

internal sealed class CreateConsignmentItemCommandHandler(InventoryDbContext db, IItemIdGeneratorService idGenerator)
    : ICommandHandler<CreateConsignmentItemCommand, CreateConsignmentItemResponse>
{
    public async Task<Result<CreateConsignmentItemResponse>> Handle(
        CreateConsignmentItemCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ReceptionExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.ReceptionNotFound);

        var brandExists = await db.Brands.AnyAsync(b => b.Id == request.BrandId, cancellationToken);
        if (!brandExists)
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.BrandNotFound);

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId.Value, cancellationToken);
            if (!categoryExists)
                return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.CategoryNotFound);
        }

        if (!Enum.TryParse<ItemCondition>(request.Condition, out var condition))
            return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.InvalidCondition);

        var tagIds = request.TagIds.ToList();
        if (tagIds.Count > 0)
        {
            var existingTags = await db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync(cancellationToken);

            if (existingTags.Count != tagIds.Count)
                return Result.Failure<CreateConsignmentItemResponse>(ItemErrors.TagsNotFound);
        }

        var itemId = await idGenerator.GenerateNextIdAsync(reception.SupplierId, cancellationToken);

        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = request.Name,
            Description = request.Description,
            BrandId = request.BrandId,
            CategoryId = request.CategoryId,
            Size = request.Size,
            Color = request.Color,
            Composition = request.Composition,
            Condition = condition,
            EvaluatedPrice = request.EvaluatedPrice,
            Status = request.IsRejected ? ItemStatus.Rejected : ItemStatus.Evaluated,
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = reception.SupplierId,
            ReceptionId = reception.Id,
            CommissionPercentage = 50m,
            IsRejected = request.IsRejected,
            RejectionReason = request.RejectionReason,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        if (tagIds.Count > 0)
        {
            var tags = await db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync(cancellationToken);
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
