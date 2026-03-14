using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Queries.GetProducts;

internal sealed class GetProductsQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductListResponse>>
{
    public async Task<Result<PagedResult<ProductListResponse>>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<EcommerceProductStatus>(request.Status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Title.Contains(request.Search) || p.Slug.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListResponse(
                p.ExternalId,
                p.Slug,
                p.Title,
                p.Price,
                p.BrandName,
                p.CategoryName,
                p.Size,
                p.Color,
                p.Status.ToString(),
                p.PublishedAt,
                p.UnpublishedAt,
                p.Photos.Where(ph => ph.IsPrimary).Select(ph => ph.FilePath).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.DisplayOrder).Select(ph => ph.FilePath).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListResponse>(
            products,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling((double)totalCount / request.PageSize));
    }
}
