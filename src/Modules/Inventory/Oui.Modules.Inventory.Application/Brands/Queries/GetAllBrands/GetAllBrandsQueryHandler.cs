using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands.Queries.GetAllBrands;

internal sealed class GetAllBrandsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetAllBrandsQuery, List<BrandListResponse>>
{
    public async Task<Result<List<BrandListResponse>>> Handle(
        GetAllBrandsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Brands.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(b => b.Name.ToLower().Contains(s));
        }

        var brands = await query
            .OrderBy(b => b.Name)
            .Select(b => new BrandListResponse(
                b.ExternalId,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.Items.Count(i => !i.IsDeleted),
                b.CreatedOn))
            .ToListAsync(cancellationToken);

        return brands;
    }
}
