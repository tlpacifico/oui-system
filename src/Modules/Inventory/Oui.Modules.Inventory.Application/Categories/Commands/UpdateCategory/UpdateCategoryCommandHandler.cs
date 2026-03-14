using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateCategoryCommand, CategoryDetailResponse>
{
    public async Task<Result<CategoryDetailResponse>> Handle(
        UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == request.ExternalId, cancellationToken);

        if (category is null)
            return Result.Failure<CategoryDetailResponse>(CategoryErrors.NotFound);

        var nameExists = await db.Categories
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && c.Id != category.Id, cancellationToken);

        if (nameExists)
            return Result.Failure<CategoryDetailResponse>(CategoryErrors.NameAlreadyExists);

        long? parentId = null;
        if (request.ParentCategoryExternalId.HasValue)
        {
            var parent = await db.Categories
                .FirstOrDefaultAsync(c => c.ExternalId == request.ParentCategoryExternalId.Value, cancellationToken);

            if (parent is null)
                return Result.Failure<CategoryDetailResponse>(CategoryErrors.ParentNotFound);

            if (parent.Id == category.Id)
                return Result.Failure<CategoryDetailResponse>(CategoryErrors.CircularReference);

            parentId = parent.Id;
        }

        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim();
        category.ParentCategoryId = parentId;
        category.UpdatedOn = DateTime.UtcNow;
        category.UpdatedBy = "system";

        await db.SaveChangesAsync(cancellationToken);

        var response = await db.Categories
            .Where(c => c.Id == category.Id)
            .Select(c => new CategoryDetailResponse(
                c.ExternalId,
                c.Name,
                c.Description,
                c.ParentCategoryId.HasValue
                    ? new CategoryParentInfo(c.ParentCategory!.ExternalId, c.ParentCategory.Name)
                    : null,
                c.SubCategories
                    .Where(sc => !sc.IsDeleted)
                    .Select(sc => new CategoryChildInfo(sc.ExternalId, sc.Name))
                    .ToList(),
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn,
                c.CreatedBy,
                c.UpdatedOn,
                c.UpdatedBy))
            .FirstAsync(cancellationToken);

        return response;
    }
}
