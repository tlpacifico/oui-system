using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Domain.Enums;
using Oui.Modules.Ecommerce.Infrastructure;

namespace shs.Api.Ecommerce;

public static class EcommercePublicEndpoints
{
    public static void MapEcommercePublicEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/store").WithTags("Store").AllowAnonymous();

        group.MapGet("/products", GetProducts);
        group.MapGet("/products/{slug}", GetProductBySlug);
        group.MapGet("/brands", GetBrands);
        group.MapGet("/categories", GetCategories);
        group.MapPost("/orders", CreateOrder);
        group.MapGet("/orders/{externalId:guid}", GetOrderStatus);
    }

    private static async Task<IResult> GetProducts(
        [FromServices] EcommerceDbContext db,
        [FromQuery] string? search,
        [FromQuery] string? brand,
        [FromQuery] string? category,
        [FromQuery] string? size,
        [FromQuery] string? color,
        [FromQuery] string? condition,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string? sort,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search) || p.BrandName.Contains(search) || p.Slug.Contains(search));

        if (!string.IsNullOrWhiteSpace(brand))
            query = query.Where(p => p.BrandName == brand);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.CategoryName == category);

        if (!string.IsNullOrWhiteSpace(size))
            query = query.Where(p => p.Size == size);

        if (!string.IsNullOrWhiteSpace(color))
            query = query.Where(p => p.Color != null && p.Color.Contains(color));

        if (!string.IsNullOrWhiteSpace(condition) && Enum.TryParse<ItemCondition>(condition, true, out var conditionEnum))
            query = query.Where(p => p.Condition == conditionEnum);

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // Sorting
        query = sort?.ToLowerInvariant() switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.PublishedAt),
            "oldest" => query.OrderBy(p => p.PublishedAt),
            _ => query.OrderByDescending(p => p.PublishedAt)
        };

        var totalCount = await query.CountAsync(ct);
        var products = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListResponse
            {
                Slug = p.Slug,
                Title = p.Title,
                Price = p.Price,
                BrandName = p.BrandName,
                CategoryName = p.CategoryName,
                Size = p.Size,
                Color = p.Color,
                Condition = p.Condition.ToString(),
                PrimaryPhotoUrl = p.Photos.Where(ph => ph.IsPrimary).Select(ph => ph.FilePath).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.DisplayOrder).Select(ph => ph.FilePath).FirstOrDefault()
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            items = products,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    private static async Task<IResult> GetProductBySlug(
        string slug,
        [FromServices] EcommerceDbContext db,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .FirstOrDefaultAsync(p => p.Slug == slug, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        return Results.Ok(new ProductDetailResponse
        {
            Slug = product.Slug,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            BrandName = product.BrandName,
            CategoryName = product.CategoryName,
            Size = product.Size,
            Color = product.Color,
            Condition = product.Condition.ToString(),
            Composition = product.Composition,
            Photos = product.Photos.Select(ph => new ProductPhotoResponse
            {
                Url = ph.FilePath,
                ThumbnailUrl = ph.ThumbnailPath,
                IsPrimary = ph.IsPrimary
            }).ToList()
        });
    }

    private static async Task<IResult> GetBrands(
        [FromServices] EcommerceDbContext db,
        CancellationToken ct)
    {
        var brands = await db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published)
            .Select(p => p.BrandName)
            .Distinct()
            .OrderBy(b => b)
            .ToListAsync(ct);

        return Results.Ok(brands);
    }

    private static async Task<IResult> GetCategories(
        [FromServices] EcommerceDbContext db,
        CancellationToken ct)
    {
        var categories = await db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published && p.CategoryName != null)
            .Select(p => p.CategoryName!)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync(ct);

        return Results.Ok(categories);
    }

    private static async Task<IResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromServices] EcommerceDbContext db,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            return Results.BadRequest(new { error = "Nome é obrigatório." });

        if (string.IsNullOrWhiteSpace(request.CustomerEmail))
            return Results.BadRequest(new { error = "Email é obrigatório." });

        if (request.ProductSlugs is null || request.ProductSlugs.Count == 0)
            return Results.BadRequest(new { error = "Selecione pelo menos um produto." });

        // Find products by slug and check availability
        var products = await db.EcommerceProducts
            .Where(p => request.ProductSlugs.Contains(p.Slug) && p.Status == EcommerceProductStatus.Published)
            .ToListAsync(ct);

        if (products.Count == 0)
            return Results.BadRequest(new { error = "Nenhum dos produtos selecionados está disponível." });

        var unavailableSlugs = request.ProductSlugs.Except(products.Select(p => p.Slug)).ToList();

        // Generate order number: EC{YYYYMMDD}-{DailySequence:000}
        var today = DateTime.UtcNow;
        var datePrefix = $"EC{today:yyyyMMdd}";
        var lastOrderToday = await db.EcommerceOrders
            .Where(o => o.OrderNumber.StartsWith(datePrefix))
            .OrderByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var sequence = 1;
        if (lastOrderToday is not null)
        {
            var lastSeq = lastOrderToday.Split('-').LastOrDefault();
            if (int.TryParse(lastSeq, out var parsed))
                sequence = parsed + 1;
        }

        var orderNumber = $"{datePrefix}-{sequence:D3}";
        var totalAmount = products.Sum(p => p.Price);

        var order = new EcommerceOrderEntity
        {
            ExternalId = Guid.NewGuid(),
            OrderNumber = orderNumber,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim(),
            CustomerPhone = request.CustomerPhone?.Trim(),
            Status = EcommerceOrderStatus.Pending,
            TotalAmount = totalAmount,
            Notes = request.Notes?.Trim(),
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "customer"
        };

        foreach (var product in products)
        {
            order.Items.Add(new EcommerceOrderItemEntity
            {
                ExternalId = Guid.NewGuid(),
                ProductId = product.Id,
                ItemId = product.ItemId,
                ProductTitle = product.Title,
                Price = product.Price,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "customer"
            });

            // Reserve the product
            product.Status = EcommerceProductStatus.Reserved;
        }

        db.EcommerceOrders.Add(order);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/store/orders/{order.ExternalId}", new
        {
            order.ExternalId,
            order.OrderNumber,
            order.CustomerName,
            order.TotalAmount,
            order.ReservedAt,
            order.ExpiresAt,
            Items = order.Items.Select(i => new { i.ProductTitle, i.Price }),
            UnavailableProducts = unavailableSlugs
        });
    }

    private static async Task<IResult> GetOrderStatus(
        Guid externalId,
        [FromServices] EcommerceDbContext db,
        CancellationToken ct)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == externalId, ct);

        if (order is null)
            return Results.NotFound(new { error = "Encomenda não encontrada." });

        return Results.Ok(new
        {
            order.OrderNumber,
            Status = order.Status.ToString(),
            order.CustomerName,
            order.TotalAmount,
            order.ReservedAt,
            order.ExpiresAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.CancelledAt,
            order.CancellationReason,
            Items = order.Items.Select(i => new { i.ProductTitle, i.Price })
        });
    }

    // DTOs
    public record CreateOrderRequest(
        string CustomerName,
        string CustomerEmail,
        string? CustomerPhone,
        List<string> ProductSlugs,
        string? Notes
    );

    public class ProductListResponse
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string? PrimaryPhotoUrl { get; set; }
    }

    public class ProductDetailResponse
    {
        public string Slug { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string? Composition { get; set; }
        public List<ProductPhotoResponse> Photos { get; set; } = [];
    }

    public class ProductPhotoResponse
    {
        public string Url { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public bool IsPrimary { get; set; }
    }
}
