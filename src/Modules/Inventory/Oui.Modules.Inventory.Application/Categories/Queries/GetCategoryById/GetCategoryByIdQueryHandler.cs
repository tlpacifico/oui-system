using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories.Queries.GetCategoryById;

internal sealed class GetCategoryByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetCategoryByIdQuery, CategoryDetailResponse>
{
    public async Task<Result<CategoryDetailResponse>> Handle(
        GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .Where(c => c.ExternalId == request.ExternalId)
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
            .FirstOrDefaultAsync(cancellationToken);

        return category is null
            ? Result.Failure<CategoryDetailResponse>(CategoryErrors.NotFound)
            : category;
    }
}
