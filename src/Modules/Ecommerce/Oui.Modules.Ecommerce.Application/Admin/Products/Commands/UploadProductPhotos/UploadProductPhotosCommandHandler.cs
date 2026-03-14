using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Oui.Modules.Ecommerce.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UploadProductPhotos;

internal sealed class UploadProductPhotosCommandHandler(EcommerceDbContext db, IWebHostEnvironment env)
    : ICommandHandler<UploadProductPhotosCommand, List<UploadProductPhotoResponse>>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxSize = 10 * 1024 * 1024;

    public async Task<Result<List<UploadProductPhotoResponse>>> Handle(
        UploadProductPhotosCommand request, CancellationToken cancellationToken)
    {
        var product = await db.EcommerceProducts
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.ExternalId == request.ExternalId, cancellationToken);

        if (product is null)
            return Result.Failure<List<UploadProductPhotoResponse>>(ProductErrors.NotFound);

        if (request.Files.Count == 0)
            return Result.Failure<List<UploadProductPhotoResponse>>(ProductErrors.NoFilesProvided);

        var currentPhotoCount = product.Photos.Count;
        if (currentPhotoCount + request.Files.Count > 10)
            return Result.Failure<List<UploadProductPhotoResponse>>(ProductErrors.MaxPhotosExceeded(currentPhotoCount));

        foreach (var file in request.Files)
        {
            if (!AllowedTypes.Contains(file.ContentType.ToLower()))
                return Result.Failure<List<UploadProductPhotoResponse>>(ProductErrors.UnsupportedFileType(file.ContentType));

            if (file.Length > MaxSize)
                return Result.Failure<List<UploadProductPhotoResponse>>(ProductErrors.FileTooLarge(file.FileName));
        }

        var uploadDir = Path.Combine(env.WebRootPath, "uploads", "ecommerce", request.ExternalId.ToString());
        Directory.CreateDirectory(uploadDir);

        var nextOrder = currentPhotoCount > 0
            ? product.Photos.Max(p => p.DisplayOrder) + 1
            : 1;

        var uploadedPhotos = new List<UploadProductPhotoResponse>();

        foreach (var file in request.Files)
        {
            var photoId = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";

            var fileName = $"{photoId}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, cancellationToken);

            var relativePath = $"/uploads/ecommerce/{request.ExternalId}/{fileName}";

            var photo = new EcommerceProductPhotoEntity
            {
                ExternalId = photoId,
                ProductId = product.Id,
                FilePath = relativePath,
                ThumbnailPath = relativePath,
                DisplayOrder = nextOrder++,
                IsPrimary = currentPhotoCount == 0 && uploadedPhotos.Count == 0,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            db.Set<EcommerceProductPhotoEntity>().Add(photo);

            uploadedPhotos.Add(new UploadProductPhotoResponse(
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
