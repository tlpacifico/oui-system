using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Commands.ReorderPhotos;

internal sealed class ReorderPhotosCommandHandler(InventoryDbContext db)
    : ICommandHandler<ReorderPhotosCommand, List<PhotoInfo>>
{
    public async Task<Result<List<PhotoInfo>>> Handle(
        ReorderPhotosCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ExternalId, cancellationToken);

        if (item is null)
            return Result.Failure<List<PhotoInfo>>(ItemErrors.NotFound);

        if (request.PhotoExternalIds is null || request.PhotoExternalIds.Length == 0)
            return Result.Failure<List<PhotoInfo>>(ItemErrors.EmptyPhotoList);

        for (var i = 0; i < request.PhotoExternalIds.Length; i++)
        {
            var photo = item.Photos.FirstOrDefault(p => p.ExternalId == request.PhotoExternalIds[i]);
            if (photo is not null)
            {
                photo.DisplayOrder = i + 1;
                photo.IsPrimary = i == 0;
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var result = item.Photos
            .OrderBy(p => p.DisplayOrder)
            .Select(p => new PhotoInfo(p.ExternalId, p.FilePath, p.ThumbnailPath, p.DisplayOrder, p.IsPrimary))
            .ToList();

        return result;
    }
}
