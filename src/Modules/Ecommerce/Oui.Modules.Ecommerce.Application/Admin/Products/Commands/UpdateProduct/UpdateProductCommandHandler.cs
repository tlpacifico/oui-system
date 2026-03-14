using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(EcommerceDbContext db)
    : ICommandHandler<UpdateProductCommand, UpdateProductResponse>
{
    public async Task<Result<UpdateProductResponse>> Handle(
        UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (product is null)
            return Result.Failure<UpdateProductResponse>(ProductErrors.NotFound);

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

        await db.SaveChangesAsync(cancellationToken);

        return new UpdateProductResponse(product.ExternalId, product.Title, product.Price);
    }
}
