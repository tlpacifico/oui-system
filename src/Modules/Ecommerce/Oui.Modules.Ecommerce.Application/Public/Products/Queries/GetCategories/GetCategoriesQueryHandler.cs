using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetCategories;

internal sealed class GetCategoriesQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetCategoriesQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published && p.CategoryName != null)
            .Select(p => p.CategoryName!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(cancellationToken);

        return categories;
    }
}
