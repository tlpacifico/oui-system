using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Queries.GetProductById;

internal sealed class GetProductByIdQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetProductByIdQuery, ProductDetailResponse>
{
    public async Task<Result<ProductDetailResponse>> Handle(
        GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (product is null)
            return Result.Failure<ProductDetailResponse>(ProductErrors.NotFound);

        return new ProductDetailResponse(
            product.ExternalId,
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
            product.Status.ToString(),
            product.PublishedAt,
            product.UnpublishedAt,
            product.Photos.Select(ph => new ProductPhotoResponse(
                ph.ExternalId,
                ph.FilePath,
                ph.ThumbnailPath,
                ph.DisplayOrder,
                ph.IsPrimary)).ToList());
    }
}
