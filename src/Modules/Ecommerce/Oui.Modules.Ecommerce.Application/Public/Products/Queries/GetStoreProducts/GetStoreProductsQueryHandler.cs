using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetStoreProducts;

internal sealed class GetStoreProductsQueryHandler(EcommerceDbContext db)
    : IQueryHandler<GetStoreProductsQuery, StorePagedResult<StoreProductListResponse>>
{
    public async Task<Result<StorePagedResult<StoreProductListResponse>>> Handle(
        GetStoreProductsQuery request, CancellationToken cancellationToken)
    {
        var query = db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Title.Contains(request.Search) || p.BrandName.Contains(request.Search) || p.Slug.Contains(request.Search));

        if (!string.IsNullOrWhiteSpace(request.Brand))
            query = query.Where(p => p.BrandName == request.Brand);

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.CategoryName == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Size))
            query = query.Where(p => p.Size == request.Size);

        if (!string.IsNullOrWhiteSpace(request.Color))
            query = query.Where(p => p.Color != null && p.Color.Contains(request.Color));

        if (!string.IsNullOrWhiteSpace(request.Condition) && Enum.TryParse<ItemCondition>(request.Condition, true, out var conditionEnum))
            query = query.Where(p => p.Condition == conditionEnum);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        query = request.Sort?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.PublishedAt),
            "oldest" => query.OrderBy(p => p.PublishedAt),
            _ => query.OrderByDescending(p => p.PublishedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new StoreProductListResponse(
                p.Slug,
                p.Title,
                p.Price,
                p.BrandName,
                p.CategoryName,
                p.Size,
                p.Color,
                p.Condition.ToString(),
                p.Photos.Where(ph => ph.IsPrimary).Select(ph => ph.FilePath).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.DisplayOrder).Select(ph => ph.FilePath).FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return new StorePagedResult<StoreProductListResponse>(
            products,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling((double)totalCount / request.PageSize));
    }
}
