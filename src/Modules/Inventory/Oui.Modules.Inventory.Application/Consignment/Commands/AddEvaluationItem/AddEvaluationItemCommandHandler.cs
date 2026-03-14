using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;
using shs.Infrastructure.Services;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.AddEvaluationItem;

internal sealed class AddEvaluationItemCommandHandler(InventoryDbContext db, IItemIdGeneratorService idGenerator)
    : ICommandHandler<AddEvaluationItemCommand, EvaluationItemResponse>
{
    public async Task<Result<EvaluationItemResponse>> Handle(
        AddEvaluationItemCommand request, CancellationToken cancellationToken)
    {
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .Include(r => r.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(r => r.ExternalId == request.ReceptionExternalId, cancellationToken);

        if (reception is null)
            return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.ReceptionNotFound);

        if (reception.Status != ReceptionStatus.PendingEvaluation)
            return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.AlreadyEvaluated);

        var currentItemCount = reception.Items.Count;
        if (currentItemCount >= reception.ItemCount)
            return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.ItemLimitReached(currentItemCount, reception.ItemCount));

        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == request.BrandExternalId, cancellationToken);
        if (brand is null)
            return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.BrandNotFound);

        long? categoryId = null;
        if (request.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == request.CategoryExternalId.Value, cancellationToken);
            if (category is null)
                return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.CategoryNotFound);
            categoryId = category.Id;
        }

        if (!Enum.TryParse<ItemCondition>(request.Condition, out var condition))
            return Result.Failure<EvaluationItemResponse>(ConsignmentErrors.InvalidCondition);

        var itemId = await idGenerator.GenerateNextIdAsync(reception.SupplierId, cancellationToken);

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
            Status = request.IsRejected ? ItemStatus.Rejected : ItemStatus.Evaluated,
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = reception.SupplierId,
            ReceptionId = reception.Id,
            CommissionPercentage = request.CommissionPercentage ?? 50m,
            IsRejected = request.IsRejected,
            RejectionReason = request.IsRejected ? request.RejectionReason?.Trim() : null,
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

        await db.Entry(item).Reference(i => i.Brand).LoadAsync(cancellationToken);

        return new EvaluationItemResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Brand.Name,
            item.Size,
            item.Color,
            item.Condition.ToString(),
            item.EvaluatedPrice,
            item.CommissionPercentage,
            item.Status.ToString(),
            item.IsRejected,
            item.RejectionReason,
            item.CreatedOn);
    }
}
