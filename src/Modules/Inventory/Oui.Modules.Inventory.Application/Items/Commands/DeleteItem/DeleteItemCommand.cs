using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Items.Commands.DeleteItem;

public sealed record DeleteItemCommand(Guid ExternalId) : ICommand;
