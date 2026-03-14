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

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishItem;

internal sealed class PublishItemCommandHandler(
    EcommerceDbContext ecommerceDb,
    InventoryDbContext inventoryDb)
    : ICommandHandler<PublishItemCommand, PublishItemResponse>
{
    public async Task<Result<PublishItemResponse>> Handle(
        PublishItemCommand request, CancellationToken cancellationToken)
    {
        var item = await inventoryDb.Items
            .Include(i => i.Brand)
            .Include(i => i.Category)
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ItemExternalId, cancellationToken);

        if (item is null)
            return Result.Failure<PublishItemResponse>(ProductErrors.ItemNotFound);

        if (item.Status != ItemStatus.ToSell)
            return Result.Failure<PublishItemResponse>(ProductErrors.ItemNotToSell);

        var alreadyPublished = await ecommerceDb.EcommerceProducts
            .AnyAsync(p => p.ItemId == item.Id &&
                (p.Status == EcommerceProductStatus.Published || p.Status == EcommerceProductStatus.Draft),
                cancellationToken);

        if (alreadyPublished)
            return Result.Failure<PublishItemResponse>(ProductErrors.AlreadyPublished);

        var product = CreateProductFromItem(item);
        ecommerceDb.EcommerceProducts.Add(product);
        await ecommerceDb.SaveChangesAsync(cancellationToken);

        return new PublishItemResponse(
            product.ExternalId,
            product.Slug,
            product.Title,
            product.Price,
            product.Status.ToString());
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
