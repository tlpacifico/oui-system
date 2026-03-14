using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateCategoryCommand, CategoryDetailResponse>
{
    public async Task<Result<CategoryDetailResponse>> Handle(
        CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await db.Categories
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
            return Result.Failure<CategoryDetailResponse>(CategoryErrors.NameAlreadyExists);

        long? parentId = null;
        CategoryParentInfo? parentInfo = null;

        if (request.ParentCategoryExternalId.HasValue)
        {
            var parent = await db.Categories
                .FirstOrDefaultAsync(c => c.ExternalId == request.ParentCategoryExternalId.Value, cancellationToken);

            if (parent is null)
                return Result.Failure<CategoryDetailResponse>(CategoryErrors.ParentNotFound);

            parentId = parent.Id;
            parentInfo = new CategoryParentInfo(parent.ExternalId, parent.Name);
        }

        var category = new CategoryEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ParentCategoryId = parentId,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryDetailResponse(
            category.ExternalId,
            category.Name,
            category.Description,
            parentInfo,
            new List<CategoryChildInfo>(),
            0,
            category.CreatedOn,
            category.CreatedBy,
            category.UpdatedOn,
            category.UpdatedBy);
    }
}
