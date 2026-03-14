using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;

namespace shs.Api.Ecommerce;

public static class EcommerceAdminEndpoints
{
    public static void MapEcommerceAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/ecommerce/admin").WithTags("Ecommerce Admin");

        group.MapPost("/publish", PublishItem).RequirePermission("ecommerce.products.publish");
        group.MapPost("/publish-batch", PublishBatch).RequirePermission("ecommerce.products.publish");
        group.MapGet("/products", GetProducts).RequirePermission("ecommerce.products.view");
        group.MapGet("/products/{externalId:guid}", GetProductById).RequirePermission("ecommerce.products.view");
        group.MapPut("/products/{externalId:guid}", UpdateProduct).RequirePermission("ecommerce.products.update");
        group.MapDelete("/products/{externalId:guid}", UnpublishProduct).RequirePermission("ecommerce.products.unpublish");
        group.MapPost("/products/{externalId:guid}/photos", UploadProductPhotos).RequirePermission("ecommerce.products.update").DisableAntiforgery();
        group.MapDelete("/products/{externalId:guid}/photos/{photoExternalId:guid}", DeleteProductPhoto).RequirePermission("ecommerce.products.update");
        group.MapGet("/orders", GetOrders).RequirePermission("ecommerce.orders.view");
        group.MapGet("/orders/{externalId:guid}", GetOrderById).RequirePermission("ecommerce.orders.view");
        group.MapPut("/orders/{externalId:guid}/confirm", ConfirmOrder).RequirePermission("ecommerce.orders.manage");
        group.MapPut("/orders/{externalId:guid}/cancel", CancelOrder).RequirePermission("ecommerce.orders.manage");
    }

    private static async Task<IResult> PublishItem(
        [FromBody] PublishItemRequest request,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ItemExternalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        if (item.Status != ItemStatus.ToSell)
            return Results.BadRequest(new { error = "Item deve estar com status 'À Venda' para ser publicado." });

        // Check if already published
        var alreadyPublished = await db.EcommerceProducts
            .AnyAsync(p => p.ItemId == item.Id &&
                (p.Status == EcommerceProductStatus.Published || p.Status == EcommerceProductStatus.Draft), ct);

        if (alreadyPublished)
            return Results.Conflict(new { error = "Este item já está publicado no e-commerce." });

        var product = CreateProductFromItem(item);
        db.EcommerceProducts.Add(product);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/ecommerce/admin/products/{product.ExternalId}", new
        {
            product.ExternalId,
            product.Slug,
            product.Title,
            product.Price,
            product.Status
        });
    }

    private static async Task<IResult> PublishBatch(
        [FromBody] PublishBatchRequest request,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        if (request.ItemExternalIds is null || request.ItemExternalIds.Count == 0)
            return Results.BadRequest(new { error = "Nenhum item fornecido." });

        var items = await db.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Photos)
            .Where(i => request.ItemExternalIds.Contains(i.ExternalId))
            .ToListAsync(ct);

        var existingItemIds = await db.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published || p.Status == EcommerceProductStatus.Draft)
            .Select(p => p.ItemId)
            .ToListAsync(ct);

        var published = new List<object>();
        var errors = new List<object>();

        foreach (var item in items)
        {
            if (item.Status != ItemStatus.ToSell)
            {
                errors.Add(new { item.ExternalId, error = "Item não está 'À Venda'." });
                continue;
            }

            if (existingItemIds.Contains(item.Id))
            {
                errors.Add(new { item.ExternalId, error = "Item já publicado." });
                continue;
            }

            var product = CreateProductFromItem(item);
            db.EcommerceProducts.Add(product);
            published.Add(new { item.ExternalId, product.Slug, product.Title });
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { published, errors, totalPublished = published.Count, totalErrors = errors.Count });
    }

    private static async Task<IResult> GetProducts(
        [FromServices] ShsDbContext db,
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EcommerceProductStatus>(status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Title.Contains(search) || p.Slug.Contains(search));

        var totalCount = await query.CountAsync(ct);
        var products = await query
            .OrderByDescending(p => p.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.ExternalId,
                p.Slug,
                p.Title,
                p.Price,
                p.BrandName,
                p.CategoryName,
                p.Size,
                p.Color,
                Status = p.Status.ToString(),
                p.PublishedAt,
                p.UnpublishedAt,
                PrimaryPhotoUrl = p.Photos.Where(ph => ph.IsPrimary).Select(ph => ph.FilePath).FirstOrDefault()
                    ?? p.Photos.OrderBy(ph => ph.DisplayOrder).Select(ph => ph.FilePath).FirstOrDefault()
            })
            .ToListAsync(ct);

        return Results.Ok(new { items = products, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    private static async Task<IResult> GetProductById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos.OrderBy(ph => ph.DisplayOrder))
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        return Results.Ok(new
        {
            product.ExternalId,
            product.Slug,
            product.Title,
            product.Description,
            product.Price,
            product.BrandName,
            product.CategoryName,
            product.Size,
            product.Color,
            Condition = product.Condition.ToString(),
            product.Composition,
            Status = product.Status.ToString(),
            product.PublishedAt,
            product.UnpublishedAt,
            Photos = product.Photos.Select(ph => new
            {
                ph.ExternalId,
                ph.FilePath,
                ph.ThumbnailPath,
                ph.DisplayOrder,
                ph.IsPrimary
            })
        });
    }

    private static async Task<IResult> UpdateProduct(
        Guid externalId,
        [FromBody] UpdateProductRequest request,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        if (!string.IsNullOrWhiteSpace(request.Title))
            product.Title = request.Title;

        if (request.Description is not null)
            product.Description = request.Description;

        if (request.Price.HasValue && request.Price.Value > 0)
            product.Price = request.Price.Value;

        if (request.BrandName is not null)
            product.BrandName = request.BrandName;

        if (request.CategoryName is not null)
            product.CategoryName = request.CategoryName;

        if (request.Size is not null)
            product.Size = request.Size;

        if (request.Color is not null)
            product.Color = request.Color;

        if (!string.IsNullOrWhiteSpace(request.Condition) && Enum.TryParse<ItemCondition>(request.Condition, true, out var condition))
            product.Condition = condition;

        if (request.Composition is not null)
            product.Composition = request.Composition;

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { product.ExternalId, product.Title, product.Price });
    }

    private static async Task<IResult> UnpublishProduct(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        if (product.Status == EcommerceProductStatus.Unpublished)
            return Results.BadRequest(new { error = "Produto já está despublicado." });

        product.Status = EcommerceProductStatus.Unpublished;
        product.UnpublishedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { product.ExternalId, message = "Produto despublicado com sucesso." });
    }

    private static async Task<IResult> GetOrders(
        [FromServices] ShsDbContext db,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.EcommerceOrders
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EcommerceOrderStatus>(status, true, out var statusEnum))
            query = query.Where(o => o.Status == statusEnum);

        var totalCount = await query.CountAsync(ct);
        var orders = await query
            .OrderByDescending(o => o.ReservedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new
            {
                o.ExternalId,
                o.OrderNumber,
                o.CustomerName,
                o.CustomerEmail,
                o.CustomerPhone,
                Status = o.Status.ToString(),
                o.TotalAmount,
                o.ReservedAt,
                o.ExpiresAt,
                o.ConfirmedAt,
                ItemCount = o.Items.Count
            })
            .ToListAsync(ct);

        return Results.Ok(new { items = orders, totalCount, page, pageSize, totalPages = (int)Math.Ceiling((double)totalCount / pageSize) });
    }

    private static async Task<IResult> GetOrderById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == externalId, ct);

        if (order is null)
            return Results.NotFound(new { error = "Encomenda não encontrada." });

        return Results.Ok(new
        {
            order.ExternalId,
            order.OrderNumber,
            order.CustomerName,
            order.CustomerEmail,
            order.CustomerPhone,
            Status = order.Status.ToString(),
            order.TotalAmount,
            order.Notes,
            order.ReservedAt,
            order.ExpiresAt,
            order.ConfirmedAt,
            order.CompletedAt,
            order.CancelledAt,
            order.CancellationReason,
            Items = order.Items.Select(i => new
            {
                i.ExternalId,
                i.ProductTitle,
                i.Price
            })
        });
    }

    private static async Task<IResult> ConfirmOrder(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var order = await db.EcommerceOrders
            .FirstOrDefaultAsync(o => o.ExternalId == externalId, ct);

        if (order is null)
            return Results.NotFound(new { error = "Encomenda não encontrada." });

        if (order.Status != EcommerceOrderStatus.Pending)
            return Results.BadRequest(new { error = "Apenas encomendas pendentes podem ser confirmadas." });

        order.Status = EcommerceOrderStatus.Confirmed;
        order.ConfirmedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { order.ExternalId, order.OrderNumber, message = "Encomenda confirmada com sucesso." });
    }

    private static async Task<IResult> CancelOrder(
        Guid externalId,
        [FromBody] CancelOrderRequest? request,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var order = await db.EcommerceOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.ExternalId == externalId, ct);

        if (order is null)
            return Results.NotFound(new { error = "Encomenda não encontrada." });

        if (order.Status == EcommerceOrderStatus.Completed || order.Status == EcommerceOrderStatus.Cancelled)
            return Results.BadRequest(new { error = "Esta encomenda não pode ser cancelada." });

        order.Status = EcommerceOrderStatus.Cancelled;
        order.CancelledAt = DateTime.UtcNow;
        order.CancellationReason = request?.Reason ?? "Cancelado pelo staff";

        // Release reserved products back to Published
        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var reservedProducts = await db.EcommerceProducts
            .Where(p => productIds.Contains(p.Id) && p.Status == EcommerceProductStatus.Reserved)
            .ToListAsync(ct);

        foreach (var product in reservedProducts)
            product.Status = EcommerceProductStatus.Published;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new { order.ExternalId, order.OrderNumber, message = "Encomenda cancelada." });
    }

    private static async Task<IResult> UploadProductPhotos(
        Guid externalId,
        [FromForm] IFormFileCollection files,
        [FromServices] ShsDbContext db,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        if (files.Count == 0)
            return Results.BadRequest(new { error = "Nenhum ficheiro enviado." });

        var currentPhotoCount = product.Photos.Count;
        if (currentPhotoCount + files.Count > 10)
            return Results.BadRequest(new { error = $"Máximo de 10 fotos por produto. Atualmente tem {currentPhotoCount}." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        const long maxSize = 10 * 1024 * 1024;

        foreach (var file in files)
        {
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return Results.BadRequest(new { error = $"Tipo de ficheiro não suportado: {file.ContentType}. Use JPEG, PNG ou WebP." });

            if (file.Length > maxSize)
                return Results.BadRequest(new { error = $"Ficheiro demasiado grande: {file.FileName}. Máximo 10 MB." });
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "ecommerce", externalId.ToString());
        Directory.CreateDirectory(uploadDir);

        var nextOrder = currentPhotoCount > 0
            ? product.Photos.Max(p => p.DisplayOrder) + 1
            : 1;

        var uploadedPhotos = new List<object>();

        foreach (var file in files)
        {
            var photoId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";

            var fileName = $"{photoId}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            var relativePath = $"/uploads/ecommerce/{externalId}/{fileName}";

            var photo = new EcommerceProductPhotoEntity
            {
                ExternalId = photoId,
                ProductId = product.Id,
                FilePath = relativePath,
                ThumbnailPath = relativePath,
                DisplayOrder = nextOrder++,
                IsPrimary = currentPhotoCount == 0 && uploadedPhotos.Count == 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            db.Set<EcommerceProductPhotoEntity>().Add(photo);

            uploadedPhotos.Add(new
            {
                photo.ExternalId,
                photo.FilePath,
                photo.ThumbnailPath,
                photo.DisplayOrder,
                photo.IsPrimary
            });
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(uploadedPhotos);
    }

    private static async Task<IResult> DeleteProductPhoto(
        Guid externalId,
        Guid photoExternalId,
        [FromServices] ShsDbContext db,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.ExternalId == externalId, ct);

        if (product is null)
            return Results.NotFound(new { error = "Produto não encontrado." });

        var photo = product.Photos.FirstOrDefault(p => p.ExternalId == photoExternalId);
        if (photo is null)
            return Results.NotFound(new { error = "Foto não encontrada." });

        var absolutePath = Path.Combine(env.WebRootPath, photo.FilePath.TrimStart('/'));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        var wasPrimary = photo.IsPrimary;
        db.Set<EcommerceProductPhotoEntity>().Remove(photo);

        if (wasPrimary)
        {
            var nextPrimary = product.Photos
                .Where(p => p.ExternalId != photoExternalId)
                .OrderBy(p => p.DisplayOrder)
                .FirstOrDefault();

            if (nextPrimary is not null)
                nextPrimary.IsPrimary = true;
        }

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    // Helper methods

    private static EcommerceProductEntity CreateProductFromItem(ItemEntity item)
    {
        var slug = GenerateSlug(item.Name, item.ExternalId);

        var product = new EcommerceProductEntity
        {
            ExternalId = Guid.NewGuid(),
            ItemId = item.Id,
            Slug = slug,
            Title = item.Name,
            Description = item.Description,
            Price = item.EvaluatedPrice,
            BrandName = item.Brand.Name,
            CategoryName = item.Category?.Name,
            Size = item.Size,
            Color = item.Color,
            Condition = item.Condition,
            Composition = item.Composition,
            Status = EcommerceProductStatus.Published,
            PublishedAt = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        // Copy photos from item
        if (item.Photos.Any())
        {
            foreach (var photo in item.Photos.OrderBy(p => p.DisplayOrder))
            {
                product.Photos.Add(new EcommerceProductPhotoEntity
                {
                    ExternalId = Guid.NewGuid(),
                    FilePath = photo.FilePath,
                    ThumbnailPath = photo.ThumbnailPath,
                    DisplayOrder = photo.DisplayOrder,
                    IsPrimary = photo.IsPrimary,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "system"
                });
            }
        }

        return product;
    }

    private static string GenerateSlug(string title, Guid externalId)
    {
        // Normalize and remove diacritics
        var normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        // Replace spaces and non-alphanumeric with hyphens
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = slug.Trim('-');

        // Append short unique suffix from ExternalId
        var shortId = externalId.ToString("N")[..6];
        return $"{slug}-{shortId}";
    }

    // Request/Response DTOs
    public record PublishItemRequest(Guid ItemExternalId);
    public record PublishBatchRequest(List<Guid> ItemExternalIds);
    public record UpdateProductRequest(
        string? Title,
        string? Description,
        decimal? Price,
        string? BrandName,
        string? CategoryName,
        string? Size,
        string? Color,
        string? Condition,
        string? Composition);
    public record CancelOrderRequest(string? Reason);
}
