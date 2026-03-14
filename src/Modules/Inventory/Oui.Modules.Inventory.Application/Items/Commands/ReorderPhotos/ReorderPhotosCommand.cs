using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.ReorderPhotos;

public sealed record ReorderPhotosCommand(Guid ExternalId, Guid[] PhotoExternalIds) : ICommand<List<PhotoInfo>>;
