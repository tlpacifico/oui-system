using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands.Queries.GetBrandById;

internal sealed class GetBrandByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetBrandByIdQuery, BrandDetailResponse>
{
    public async Task<Result<BrandDetailResponse>> Handle(
        GetBrandByIdQuery request, CancellationToken cancellationToken)
    {
        var brand = await db.Brands
            .Where(b => b.ExternalId == request.ExternalId)
            .Select(b => new BrandDetailResponse(
                b.ExternalId,
                b.Name,
                b.Description,
                b.LogoUrl,
                b.Items.Count(i => !i.IsDeleted),
                b.CreatedOn,
                b.CreatedBy,
                b.UpdatedOn,
                b.UpdatedBy))
            .FirstOrDefaultAsync(cancellationToken);

        return brand is null
            ? Result.Failure<BrandDetailResponse>(BrandErrors.NotFound)
            : brand;
    }
}
