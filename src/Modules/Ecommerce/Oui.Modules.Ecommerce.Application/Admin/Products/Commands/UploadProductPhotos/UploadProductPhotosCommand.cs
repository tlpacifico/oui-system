using Microsoft.AspNetCore.Http;
using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UploadProductPhotos;

public sealed record UploadProductPhotosCommand(Guid ExternalId, IFormFileCollection Files)
    : ICommand<List<UploadProductPhotoResponse>>;
