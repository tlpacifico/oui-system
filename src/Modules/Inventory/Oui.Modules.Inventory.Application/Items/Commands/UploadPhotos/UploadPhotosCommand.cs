using Microsoft.AspNetCore.Http;
using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.UploadPhotos;

public sealed record UploadPhotosCommand(Guid ExternalId, IFormFileCollection Files) : ICommand<List<PhotoInfo>>;
