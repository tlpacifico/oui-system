using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.DeletePhoto;

public sealed record DeletePhotoCommand(Guid ItemExternalId, Guid PhotoExternalId) : ICommand;
