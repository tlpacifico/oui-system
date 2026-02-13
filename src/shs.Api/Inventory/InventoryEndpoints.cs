using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shs.Api.Authorization;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Infrastructure.Database;
using shs.Infrastructure.Services;

namespace shs.Api.Inventory;

public static class InventoryEndpoints
{
    public static void MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").WithTags("Inventory");

        group.MapPost("/items", CreateItem).RequirePermission("inventory.items.create");
        group.MapPost("/items/consignment", CreateConsignmentItem).RequirePermission("inventory.items.create");
        group.MapGet("/items", GetItems).RequirePermission("inventory.items.view");
        group.MapGet("/items/{externalId:guid}", GetItemById).RequirePermission("inventory.items.view");
        group.MapPut("/items/{externalId:guid}", UpdateItem).RequirePermission("inventory.items.update");
        group.MapDelete("/items/{externalId:guid}", DeleteItem).RequirePermission("inventory.items.delete");

        // Photo endpoints
        group.MapPost("/items/{externalId:guid}/photos", UploadPhotos).RequirePermission("inventory.items.update").DisableAntiforgery();
        group.MapDelete("/items/{itemExternalId:guid}/photos/{photoExternalId:guid}", DeletePhoto).RequirePermission("inventory.items.update");
        group.MapPut("/items/{externalId:guid}/photos/reorder", ReorderPhotos).RequirePermission("inventory.items.update");
    }

    private static async Task<IResult> CreateItem(
        [FromBody] CreateItemRequest req,
        [FromServices] ShsDbContext db,
        [FromServices] IItemIdGeneratorService idGenerator,
        CancellationToken ct)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "O nome é obrigatório." });
        if (req.EvaluatedPrice <= 0)
            return Results.BadRequest(new { error = "O preço deve ser maior que zero." });
        if (string.IsNullOrWhiteSpace(req.Size))
            return Results.BadRequest(new { error = "O tamanho é obrigatório." });
        if (string.IsNullOrWhiteSpace(req.Color))
            return Results.BadRequest(new { error = "A cor é obrigatória." });

        // Validate brand
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == req.BrandExternalId, ct);
        if (brand is null)
            return Results.BadRequest(new { error = "Marca não encontrada." });

        // Validate category (optional)
        long? categoryId = null;
        if (req.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == req.CategoryExternalId.Value, ct);
            if (category is null)
                return Results.BadRequest(new { error = "Categoria não encontrada." });
            categoryId = category.Id;
        }

        // Validate condition
        if (!Enum.TryParse<ItemCondition>(req.Condition, out var condition))
            return Results.BadRequest(new { error = "Condição inválida." });

        // Validate acquisition type
        if (!Enum.TryParse<AcquisitionType>(req.AcquisitionType, out var acquisitionType))
            return Results.BadRequest(new { error = "Tipo de aquisição inválido." });

        // Validate origin
        if (!Enum.TryParse<ItemOrigin>(req.Origin, out var origin))
            return Results.BadRequest(new { error = "Origem inválida." });

        // Validate supplier for consignment items
        long? supplierId = null;
        if (acquisitionType == AcquisitionType.Consignment)
        {
            if (!req.SupplierExternalId.HasValue)
                return Results.BadRequest(new { error = "Fornecedor é obrigatório para itens de consignação." });

            var supplier = await db.Suppliers.FirstOrDefaultAsync(s => s.ExternalId == req.SupplierExternalId.Value, ct);
            if (supplier is null)
                return Results.BadRequest(new { error = "Fornecedor não encontrado." });
            supplierId = supplier.Id;
        }

        // Generate ID
        var itemId = await idGenerator.GenerateNextIdAsync(supplierId, ct);

        // Determine initial status
        var status = acquisitionType == AcquisitionType.OwnPurchase ? ItemStatus.ToSell : ItemStatus.Evaluated;

        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = req.Name.Trim(),
            Description = req.Description?.Trim(),
            BrandId = brand.Id,
            CategoryId = categoryId,
            Size = req.Size.Trim(),
            Color = req.Color.Trim(),
            Composition = req.Composition?.Trim(),
            Condition = condition,
            EvaluatedPrice = req.EvaluatedPrice,
            CostPrice = req.CostPrice,
            Status = status,
            AcquisitionType = acquisitionType,
            Origin = origin,
            SupplierId = supplierId,
            CommissionPercentage = req.CommissionPercentage ?? 50m,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        // Add tags
        if (req.TagExternalIds is { Length: > 0 })
        {
            var tags = await db.Tags
                .Where(t => req.TagExternalIds.Contains(t.ExternalId))
                .ToListAsync(ct);
            item.Tags = tags;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/inventory/items/{item.ExternalId}",
            new CreateConsignmentItemResponse(
                item.ExternalId,
                item.IdentificationNumber,
                item.Name,
                item.Status.ToString(),
                item.CreatedOn
            )
        );
    }

    private static async Task<IResult> CreateConsignmentItem(
        [FromBody] CreateConsignmentItemRequest req,
        [FromServices] ShsDbContext db,
        [FromServices] IItemIdGeneratorService idGenerator,
        CancellationToken ct)
    {
        // Validate reception exists
        var reception = await db.Receptions
            .Include(r => r.Supplier)
            .FirstOrDefaultAsync(r => r.ExternalId == req.ReceptionExternalId, ct);

        if (reception == null)
            return Results.NotFound(new { error = "Reception not found" });

        // Validate brand exists
        var brandExists = await db.Brands.AnyAsync(b => b.Id == req.BrandId, ct);
        if (!brandExists)
            return Results.BadRequest(new { error = "Brand not found" });

        // Validate category if provided
        if (req.CategoryId.HasValue)
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == req.CategoryId.Value, ct);
            if (!categoryExists)
                return Results.BadRequest(new { error = "Category not found" });
        }

        // Validate condition
        if (!Enum.TryParse<ItemCondition>(req.Condition, out var condition))
            return Results.BadRequest(new { error = "Invalid condition value" });

        // Validate tags exist
        var tagIds = req.TagIds.ToList();
        if (tagIds.Any())
        {
            var existingTags = await db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync(ct);

            if (existingTags.Count != tagIds.Count)
                return Results.BadRequest(new { error = "One or more tags not found" });
        }

        // Generate ID
        var itemId = await idGenerator.GenerateNextIdAsync(reception.SupplierId, ct);

        // Create item
        var item = new ItemEntity
        {
            ExternalId = Guid.NewGuid(),
            IdentificationNumber = itemId,
            Name = req.Name,
            Description = req.Description,
            BrandId = req.BrandId,
            CategoryId = req.CategoryId,
            Size = req.Size,
            Color = req.Color,
            Composition = req.Composition,
            Condition = condition,
            EvaluatedPrice = req.EvaluatedPrice,
            Status = req.IsRejected ? ItemStatus.Rejected : ItemStatus.Evaluated,
            AcquisitionType = AcquisitionType.Consignment,
            Origin = ItemOrigin.Consignment,
            SupplierId = reception.SupplierId,
            ReceptionId = reception.Id,
            CommissionPercentage = 50m, // Default 50%
            IsRejected = req.IsRejected,
            RejectionReason = req.RejectionReason,
            DaysInStock = 0,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system" // TODO: Get from JWT claims
        };

        // Add tags
        if (tagIds.Any())
        {
            var tags = await db.Tags.Where(t => tagIds.Contains(t.Id)).ToListAsync(ct);
            item.Tags = tags;
        }

        db.Items.Add(item);
        await db.SaveChangesAsync(ct);

        return Results.Created(
            $"/api/inventory/items/{item.ExternalId}",
            new CreateConsignmentItemResponse(
                item.ExternalId,
                item.IdentificationNumber,
                item.Name,
                item.Status.ToString(),
                item.CreatedOn
            )
        );
    }

    private static async Task<IResult> GetItems(
        [FromServices] ShsDbContext db,
        [FromQuery] string? search,
        [FromQuery] long? brandId,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = db.Items
            .Include(i => i.Brand)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder).Take(1))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(s) ||
                i.IdentificationNumber.ToLower().Contains(s));
        }

        if (brandId.HasValue)
            query = query.Where(i => i.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ItemStatus>(status, out var itemStatus))
            query = query.Where(i => i.Status == itemStatus);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(i => i.CreatedOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemListItemResponse(
                i.ExternalId,
                i.IdentificationNumber,
                i.Name,
                i.Brand.Name,
                i.Size,
                i.Color,
                i.EvaluatedPrice,
                i.Status.ToString(),
                i.Photos.OrderBy(p => p.DisplayOrder).Select(p => p.ThumbnailPath ?? p.FilePath).FirstOrDefault(),
                i.CreatedOn
            ))
            .ToListAsync(ct);

        return Results.Ok(new PagedResult<ItemListItemResponse>(
            items,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling(totalCount / (double)pageSize)
        ));
    }

    private static async Task<IResult> GetItemById(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Supplier)
            .Include(i => i.Tags)
            .Include(i => i.Photos.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item == null)
            return Results.NotFound();

        return Results.Ok(new ItemDetailResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Description,
            new BrandInfo(item.Brand.Id, item.Brand.Name),
            item.Category != null ? new CategoryInfo(item.Category.Id, item.Category.Name) : null,
            item.Size,
            item.Color,
            item.Composition,
            item.Condition.ToString(),
            item.EvaluatedPrice,
            item.CostPrice,
            item.FinalSalePrice,
            item.Status.ToString(),
            item.AcquisitionType.ToString(),
            item.Origin.ToString(),
            item.Supplier != null ? new SupplierInfo(item.Supplier.Id, item.Supplier.Name) : null,
            item.CommissionPercentage,
            item.CommissionAmount,
            item.IsRejected,
            item.RejectionReason,
            item.SoldAt,
            item.DaysInStock,
            item.Tags.Select(t => new TagInfo(t.Id, t.Name, t.Color)).ToList(),
            item.Photos.Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary)).ToList(),
            item.CreatedOn,
            item.CreatedBy,
            item.UpdatedOn,
            item.UpdatedBy
        ));
    }

    private static async Task<IResult> UpdateItem(
        Guid externalId,
        [FromBody] UpdateItemRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Tags)
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        // Cannot edit sold items
        if (item.Status == ItemStatus.Sold)
            return Results.Conflict(new { error = "Não é possível editar um item já vendido." });

        // Validate required fields
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest(new { error = "O nome é obrigatório." });
        if (req.EvaluatedPrice <= 0)
            return Results.BadRequest(new { error = "O preço deve ser maior que zero." });
        if (string.IsNullOrWhiteSpace(req.Size))
            return Results.BadRequest(new { error = "O tamanho é obrigatório." });
        if (string.IsNullOrWhiteSpace(req.Color))
            return Results.BadRequest(new { error = "A cor é obrigatória." });

        // Validate brand
        var brand = await db.Brands.FirstOrDefaultAsync(b => b.ExternalId == req.BrandExternalId, ct);
        if (brand is null)
            return Results.BadRequest(new { error = "Marca não encontrada." });

        // Validate category (optional)
        long? categoryId = null;
        if (req.CategoryExternalId.HasValue)
        {
            var category = await db.Categories.FirstOrDefaultAsync(c => c.ExternalId == req.CategoryExternalId.Value, ct);
            if (category is null)
                return Results.BadRequest(new { error = "Categoria não encontrada." });
            categoryId = category.Id;
        }

        // Validate condition
        if (!Enum.TryParse<ItemCondition>(req.Condition, out var condition))
            return Results.BadRequest(new { error = "Condição inválida." });

        // Update fields
        item.Name = req.Name.Trim();
        item.Description = req.Description?.Trim();
        item.BrandId = brand.Id;
        item.CategoryId = categoryId;
        item.Size = req.Size.Trim();
        item.Color = req.Color.Trim();
        item.Composition = req.Composition?.Trim();
        item.Condition = condition;
        item.EvaluatedPrice = req.EvaluatedPrice;
        item.CostPrice = req.CostPrice;
        item.CommissionPercentage = req.CommissionPercentage ?? item.CommissionPercentage;
        item.UpdatedOn = DateTime.UtcNow;
        item.UpdatedBy = "system"; // TODO: Get from JWT claims

        // Update tags
        if (req.TagExternalIds is not null)
        {
            item.Tags.Clear();
            if (req.TagExternalIds.Length > 0)
            {
                var tags = await db.Tags
                    .Where(t => req.TagExternalIds.Contains(t.ExternalId))
                    .ToListAsync(ct);
                item.Tags = tags;
            }
        }

        await db.SaveChangesAsync(ct);

        // Reload for response
        await db.Entry(item).Reference(i => i.Brand).LoadAsync(ct);
        await db.Entry(item).Reference(i => i.Category).LoadAsync(ct);
        await db.Entry(item).Reference(i => i.Supplier).LoadAsync(ct);
        await db.Entry(item).Collection(i => i.Tags).LoadAsync(ct);
        await db.Entry(item).Collection(i => i.Photos).LoadAsync(ct);

        return Results.Ok(new ItemDetailResponse(
            item.ExternalId,
            item.IdentificationNumber,
            item.Name,
            item.Description,
            new BrandInfo(item.Brand.Id, item.Brand.Name),
            item.Category != null ? new CategoryInfo(item.Category.Id, item.Category.Name) : null,
            item.Size,
            item.Color,
            item.Composition,
            item.Condition.ToString(),
            item.EvaluatedPrice,
            item.CostPrice,
            item.FinalSalePrice,
            item.Status.ToString(),
            item.AcquisitionType.ToString(),
            item.Origin.ToString(),
            item.Supplier != null ? new SupplierInfo(item.Supplier.Id, item.Supplier.Name) : null,
            item.CommissionPercentage,
            item.CommissionAmount,
            item.IsRejected,
            item.RejectionReason,
            item.SoldAt,
            item.DaysInStock,
            item.Tags.Select(t => new TagInfo(t.Id, t.Name, t.Color)).ToList(),
            item.Photos.Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary)).ToList(),
            item.CreatedOn,
            item.CreatedBy,
            item.UpdatedOn,
            item.UpdatedBy
        ));
    }

    private static async Task<IResult> DeleteItem(
        Guid externalId,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        if (item.Status == ItemStatus.Sold)
            return Results.Conflict(new { error = "Não é possível eliminar um item já vendido." });

        // Soft delete
        item.IsDeleted = true;
        item.DeletedBy = "system"; // TODO: Get from JWT claims
        item.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    // ── Photo Endpoints ──

    /// <summary>
    /// Upload one or more photos for an item.
    /// Files are saved to wwwroot/uploads/items/{externalId}/
    /// Max 10 photos per item, max 10 MB per file.
    /// </summary>
    private static async Task<IResult> UploadPhotos(
        Guid externalId,
        [FromForm] IFormFileCollection files,
        [FromServices] ShsDbContext db,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        if (files.Count == 0)
            return Results.BadRequest(new { error = "Nenhum ficheiro enviado." });

        var currentPhotoCount = item.Photos.Count;
        if (currentPhotoCount + files.Count > 10)
            return Results.BadRequest(new { error = $"Máximo de 10 fotos por item. Atualmente tem {currentPhotoCount}." });

        // Validate files
        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        const long maxSize = 10 * 1024 * 1024; // 10 MB

        foreach (var file in files)
        {
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return Results.BadRequest(new { error = $"Tipo de ficheiro não suportado: {file.ContentType}. Use JPEG, PNG ou WebP." });

            if (file.Length > maxSize)
                return Results.BadRequest(new { error = $"Ficheiro demasiado grande: {file.FileName}. Máximo 10 MB." });
        }

        // Create directory
        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "items", externalId.ToString());
        Directory.CreateDirectory(uploadDir);

        var nextOrder = currentPhotoCount > 0
            ? item.Photos.Max(p => p.DisplayOrder) + 1
            : 1;

        var uploadedPhotos = new List<PhotoInfo>();

        foreach (var file in files)
        {
            var photoId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";

            var fileName = $"{photoId}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            // Save file to disk
            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            // Relative URL path for serving
            var relativePath = $"/uploads/items/{externalId}/{fileName}";

            var photo = new ItemPhotoEntity
            {
                ExternalId = photoId,
                ItemId = item.Id,
                FileName = file.FileName,
                FilePath = relativePath,
                ThumbnailPath = relativePath, // Use same path for now (no thumbnail generation)
                DisplayOrder = nextOrder++,
                IsPrimary = currentPhotoCount == 0 && uploadedPhotos.Count == 0, // First photo is primary
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            db.Set<ItemPhotoEntity>().Add(photo);

            uploadedPhotos.Add(new PhotoInfo(
                photo.ExternalId,
                photo.FilePath,
                photo.ThumbnailPath,
                photo.DisplayOrder,
                photo.IsPrimary
            ));
        }

        await db.SaveChangesAsync(ct);

        return Results.Ok(uploadedPhotos);
    }

    /// <summary>
    /// Delete a photo from an item. Removes file from disk.
    /// </summary>
    private static async Task<IResult> DeletePhoto(
        Guid itemExternalId,
        Guid photoExternalId,
        [FromServices] ShsDbContext db,
        [FromServices] IWebHostEnvironment env,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == itemExternalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        var photo = item.Photos.FirstOrDefault(p => p.ExternalId == photoExternalId);
        if (photo is null)
            return Results.NotFound(new { error = "Foto não encontrada." });

        // Delete file from disk
        var absolutePath = Path.Combine(env.WebRootPath, photo.FilePath.TrimStart('/'));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        var wasPrimary = photo.IsPrimary;
        db.Set<ItemPhotoEntity>().Remove(photo);

        // If deleted photo was primary, make the next one primary
        if (wasPrimary)
        {
            var nextPrimary = item.Photos
                .Where(p => p.ExternalId != photoExternalId)
                .OrderBy(p => p.DisplayOrder)
                .FirstOrDefault();

            if (nextPrimary is not null)
                nextPrimary.IsPrimary = true;
        }

        await db.SaveChangesAsync(ct);

        return Results.NoContent();
    }

    /// <summary>
    /// Reorder photos for an item and set primary.
    /// Receives an array of photo externalIds in the desired order.
    /// </summary>
    private static async Task<IResult> ReorderPhotos(
        Guid externalId,
        [FromBody] ReorderPhotosRequest req,
        [FromServices] ShsDbContext db,
        CancellationToken ct)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == externalId, ct);

        if (item is null)
            return Results.NotFound(new { error = "Item não encontrado." });

        if (req.PhotoExternalIds is null || req.PhotoExternalIds.Length == 0)
            return Results.BadRequest(new { error = "Lista de fotos vazia." });

        for (var i = 0; i < req.PhotoExternalIds.Length; i++)
        {
            var photo = item.Photos.FirstOrDefault(p => p.ExternalId == req.PhotoExternalIds[i]);
            if (photo is not null)
            {
                photo.DisplayOrder = i + 1;
                photo.IsPrimary = i == 0; // First in order is primary
            }
        }

        await db.SaveChangesAsync(ct);

        var result = item.Photos
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary))
            .ToList();

        return Results.Ok(result);
    }
}

// Request DTOs
public record CreateItemRequest(
    string Name,
    string? Description,
    Guid BrandExternalId,
    Guid? CategoryExternalId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    string AcquisitionType,
    string Origin,
    Guid? SupplierExternalId,
    decimal? CommissionPercentage,
    Guid[]? TagExternalIds
);

public record UpdateItemRequest(
    string Name,
    string? Description,
    Guid BrandExternalId,
    Guid? CategoryExternalId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    decimal? CommissionPercentage,
    Guid[]? TagExternalIds
);

public record CreateConsignmentItemRequest(
    Guid ReceptionExternalId,
    string Name,
    string? Description,
    long BrandId,
    long? CategoryId,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    long[] TagIds,
    bool IsRejected,
    string? RejectionReason
);

// Response DTOs
public record CreateConsignmentItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Status,
    DateTime CreatedAt
);

public record ItemListItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    decimal EvaluatedPrice,
    string Status,
    string? PrimaryPhotoUrl,
    DateTime CreatedOn
);

public record ItemDetailResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string? Description,
    BrandInfo Brand,
    CategoryInfo? Category,
    string Size,
    string Color,
    string? Composition,
    string Condition,
    decimal EvaluatedPrice,
    decimal? CostPrice,
    decimal? FinalSalePrice,
    string Status,
    string AcquisitionType,
    string Origin,
    SupplierInfo? Supplier,
    decimal CommissionPercentage,
    decimal? CommissionAmount,
    bool IsRejected,
    string? RejectionReason,
    DateTime? SoldAt,
    int DaysInStock,
    List<TagInfo> Tags,
    List<PhotoInfo> Photos,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy
);

public record PagedResult<T>(
    List<T> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record BrandInfo(long Id, string Name);
public record CategoryInfo(long Id, string Name);
public record SupplierInfo(long Id, string Name);
public record TagInfo(long Id, string Name, string? Color);
public record PhotoInfo(Guid ExternalId, string FilePath, string? ThumbnailPath, int DisplayOrder, bool IsPrimary);
public record ReorderPhotosRequest(Guid[] PhotoExternalIds);
