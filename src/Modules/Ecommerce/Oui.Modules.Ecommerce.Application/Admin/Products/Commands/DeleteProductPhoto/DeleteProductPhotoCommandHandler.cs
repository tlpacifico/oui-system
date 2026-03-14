using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.DeleteProductPhoto;

internal sealed class DeleteProductPhotoCommandHandler(EcommerceDbContext db, IWebHostEnvironment env)
    : ICommandHandler<DeleteProductPhotoCommand>
{
    public async Task<Result> Handle(
        DeleteProductPhotoCommand request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (product is null)
            return Result.Failure(ProductErrors.NotFound);

        var photo = product.Photos.FirstOrDefault(p => p.ExternalId == request.PhotoExternalId);
        if (photo is null)
            return Result.Failure(ProductErrors.PhotoNotFound);

        var absolutePath = Path.Combine(env.WebRootPath, photo.FilePath.TrimStart('/'));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        var wasPrimary = photo.IsPrimary;
        db.Set<EcommerceProductPhotoEntity>().Remove(photo);

        if (wasPrimary)
        {
            var nextPrimary = product.Photos
                .Where(p => p.ExternalId != request.PhotoExternalId)
                .OrderBy(p => p.DisplayOrder)
                .FirstOrDefault();

            if (nextPrimary is not null)
                nextPrimary.IsPrimary = true;
        }

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
