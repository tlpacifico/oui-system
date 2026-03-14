using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands.Commands.CreateBrand;

internal sealed class CreateBrandCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateBrandCommand, BrandDetailResponse>
{
    public async Task<Result<BrandDetailResponse>> Handle(
        CreateBrandCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await db.Brands
            .AnyAsync(b => b.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
            return Result.Failure<BrandDetailResponse>(BrandErrors.NameAlreadyExists);

        var brand = new BrandEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            LogoUrl = request.LogoUrl?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.UserEmail
        };

        db.Brands.Add(brand);
        await db.SaveChangesAsync(cancellationToken);

        return new BrandDetailResponse(
            brand.ExternalId,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            0,
            brand.CreatedOn,
            brand.CreatedBy,
            brand.UpdatedOn,
            brand.UpdatedBy);
    }
}
