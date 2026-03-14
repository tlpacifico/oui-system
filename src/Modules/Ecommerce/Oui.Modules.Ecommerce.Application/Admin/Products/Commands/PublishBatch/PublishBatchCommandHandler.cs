using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishBatch;

internal sealed class PublishBatchCommandHandler(
    EcommerceDbContext ecommerceDb,
    InventoryDbContext inventoryDb)
    : ICommandHandler<PublishBatchCommand, PublishBatchResponse>
{
    public async Task<Result<PublishBatchResponse>> Handle(
        PublishBatchCommand request, CancellationToken cancellationToken)
    {
        if (request.ItemExternalIds is null || request.ItemExternalIds.Count == 0)
            return Result.Failure<PublishBatchResponse>(ProductErrors.NoItemsProvided);

        var items = await inventoryDb.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Photos)
            .Where(i => request.ItemExternalIds.Contains(i.ExternalId))
            .ToListAsync(cancellationToken);

        var existingItemIds = await ecommerceDb.EcommerceProducts
            .Where(p => p.Status == EcommerceProductStatus.Published || p.Status == EcommerceProductStatus.Draft)
            .Select(p => p.ItemId)
            .ToListAsync(cancellationToken);

        var published = new List<PublishBatchItemResponse>();
        var errors = new List<PublishBatchErrorResponse>();

        foreach (var item in items)
        {
            if (item.Status != ItemStatus.ToSell)
            {
                errors.Add(new PublishBatchErrorResponse(item.ExternalId, "Item não está 'À Venda'."));
                continue;
            }

            if (existingItemIds.Contains(item.Id))
            {
                errors.Add(new PublishBatchErrorResponse(item.ExternalId, "Item já publicado."));
                continue;
            }

            var product = CreateProductFromItem(item);
            ecommerceDb.EcommerceProducts.Add(product);
            published.Add(new PublishBatchItemResponse(item.ExternalId, product.Slug, product.Title));
        }

        await ecommerceDb.SaveChangesAsync(cancellationToken);

        return new PublishBatchResponse(published, errors, published.Count, errors.Count);
    }

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
        var normalized = title.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9]+", "-");
        slug = slug.Trim('-');

        var shortId = externalId.ToString("N")[..6];
        return $"{slug}-{shortId}";
    }
}
