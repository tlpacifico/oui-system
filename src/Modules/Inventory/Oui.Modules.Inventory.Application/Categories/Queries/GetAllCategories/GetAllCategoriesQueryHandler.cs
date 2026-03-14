using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories.Queries.GetAllCategories;

internal sealed class GetAllCategoriesQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetAllCategoriesQuery, List<CategoryListResponse>>
{
    public async Task<Result<List<CategoryListResponse>>> Handle(
        GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s));
        }

        var categories = await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryListResponse(
                c.ExternalId,
                c.Name,
                c.Description,
                c.ParentCategoryId.HasValue
                    ? new CategoryParentInfo(c.ParentCategory!.ExternalId, c.ParentCategory.Name)
                    : null,
                c.SubCategories.Count(sc => !sc.IsDeleted),
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
