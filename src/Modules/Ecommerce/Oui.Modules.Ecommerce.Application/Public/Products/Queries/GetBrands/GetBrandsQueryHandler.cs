using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetBrands;

internal sealed class GetBrandsQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetBrandsQuery, List<string>>
{
    public async Task<Result<List<string>>> Handle(
        GetBrandsQuery request, CancellationToken cancellationToken)
    {
        var brands = await db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .Select(p => p.BrandName)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(cancellationToken);

        return brands;
    }
}
