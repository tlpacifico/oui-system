using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands.Commands.UpdateBrand;

internal sealed class UpdateBrandCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateBrandCommand, BrandDetailResponse>
{
    public async Task<Result<BrandDetailResponse>> Handle(
        UpdateBrandCommand request, CancellationToken cancellationToken)
    {
        var brand = await db.Brands
            .FirstOrDefaultAsync(b => b.ExternalId == request.ExternalId, cancellationToken);

        if (brand is null)
            return Result.Failure<BrandDetailResponse>(BrandErrors.NotFound);

        var nameExists = await db.Brands
            .AnyAsync(b => b.Name.ToLower() == request.Name.Trim().ToLower() && b.Id != brand.Id, cancellationToken);

        if (nameExists)
            return Result.Failure<BrandDetailResponse>(BrandErrors.NameAlreadyExists);

        brand.Name = request.Name.Trim();
        brand.Description = request.Description?.Trim();
        brand.LogoUrl = request.LogoUrl?.Trim();
        brand.UpdatedOn = DateTime.UtcNow;
        brand.UpdatedBy = request.UserEmail;

        await db.SaveChangesAsync(cancellationToken);

        var itemCount = await db.Items.CountAsync(i => i.BrandId == brand.Id && !i.IsDeleted, cancellationToken);

        return new BrandDetailResponse(
            brand.ExternalId,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            itemCount,
            brand.CreatedOn,
            brand.CreatedBy,
            brand.UpdatedOn,
            brand.UpdatedBy);
    }
}
