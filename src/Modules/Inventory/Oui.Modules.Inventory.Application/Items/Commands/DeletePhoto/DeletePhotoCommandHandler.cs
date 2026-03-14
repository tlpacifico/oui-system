using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Commands.DeletePhoto;

internal sealed class DeletePhotoCommandHandler(InventoryDbContext db, IWebHostEnvironment env)
    : ICommandHandler<DeletePhotoCommand>
{
    public async Task<Result> Handle(
        DeletePhotoCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ItemExternalId, cancellationToken);

        if (item is null)
            return Result.Failure(ItemErrors.NotFound);

        var photo = item.Photos.FirstOrDefault(p => p.ExternalId == request.PhotoExternalId);
        if (photo is null)
            return Result.Failure(ItemErrors.PhotoNotFound);

        var absolutePath = Path.Combine(env.WebRootPath, photo.FilePath.TrimStart('/'));
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        var wasPrimary = photo.IsPrimary;
        db.Set<ItemPhotoEntity>().Remove(photo);

        if (wasPrimary)
        {
            var nextPrimary = item.Photos
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
