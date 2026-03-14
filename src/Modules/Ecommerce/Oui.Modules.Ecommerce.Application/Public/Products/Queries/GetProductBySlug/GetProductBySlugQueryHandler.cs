using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetProductBySlug;

internal sealed class GetProductBySlugQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetProductBySlugQuery, StoreProductDetailResponse>
{
    public async Task<Result<StoreProductDetailResponse>> Handle(
        GetProductBySlugQuery request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .FirstOrDefaultAsync(p => p.Slug == request.Slug, cancellationToken);

        if (product is null)
            return Result.Failure<StoreProductDetailResponse>(StoreProductErrors.NotFound);

        return new StoreProductDetailResponse(
            product.Slug,
            product.Title,
            product.Description,
            product.Price,
            product.BrandName,
            product.CategoryName,
            product.Size,
            product.Color,
            product.Condition.ToString(),
            product.Composition,
            product.Photos.Select(ph => new StoreProductPhotoResponse(
                ph.FilePath,
                ph.ThumbnailPath,
                ph.IsPrimary)).ToList());
    }
}
