using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Commands.UploadPhotos;

internal sealed class UploadPhotosCommandHandler(InventoryDbContext db, IWebHostEnvironment env)
    : ICommandHandler<UploadPhotosCommand, List<PhotoInfo>>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxSize = 10 * 1024 * 1024;

    public async Task<Result<List<PhotoInfo>>> Handle(
        UploadPhotosCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .Include(i => i.Photos)
            .FirstOrDefaultAsync(i => i.ExternalId == request.ExternalId, cancellationToken);

        if (item is null)
            return Result.Failure<List<PhotoInfo>>(ItemErrors.NotFound);

        if (request.Files.Count == 0)
            return Result.Failure<List<PhotoInfo>>(ItemErrors.NoFilesProvided);

        var currentPhotoCount = item.Photos.Count;
        if (currentPhotoCount + request.Files.Count > 10)
            return Result.Failure<List<PhotoInfo>>(ItemErrors.MaxPhotosExceeded(currentPhotoCount));

        foreach (var file in request.Files)
        {
            if (!AllowedTypes.Contains(file.ContentType.ToLower()))
                return Result.Failure<List<PhotoInfo>>(ItemErrors.UnsupportedFileType(file.ContentType));

            if (file.Length > MaxSize)
                return Result.Failure<List<PhotoInfo>>(ItemErrors.FileTooLarge(file.FileName));
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "items", request.ExternalId.ToString());
        Directory.CreateDirectory(uploadDir);

        var nextOrder = currentPhotoCount > 0
            ? item.Photos.Max(p => p.DisplayOrder) + 1
            : 1;

        var uploadedPhotos = new List<PhotoInfo>();

        foreach (var file in request.Files)
        {
            var photoId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";

            var fileName = $"{photoId}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var relativePath = $"/uploads/items/{request.ExternalId}/{fileName}";

            var photo = new ItemPhotoEntity
            {
                ExternalId = photoId,
                ItemId = item.Id,
                FileName = file.FileName,
                FilePath = relativePath,
                ThumbnailPath = relativePath,
                DisplayOrder = nextOrder++,
                IsPrimary = currentPhotoCount == 0 && uploadedPhotos.Count == 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            db.Set<ItemPhotoEntity>().Add(photo);

            uploadedPhotos.Add(new PhotoInfo(
                photo.ExternalId,
                photo.FilePath,
                photo.ThumbnailPath,
                photo.DisplayOrder,
                photo.IsPrimary));
        }

        await db.SaveChangesAsync(cancellationToken);

        return uploadedPhotos;
    }
}
