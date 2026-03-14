using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.DeleteProductPhoto;

public sealed record DeleteProductPhotoCommand(Guid ExternalId, Guid PhotoExternalId) : ICommand;
