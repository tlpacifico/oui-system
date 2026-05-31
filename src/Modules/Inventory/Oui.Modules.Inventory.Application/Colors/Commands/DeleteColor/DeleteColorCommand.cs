using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Colors.Commands.DeleteColor;

public sealed record DeleteColorCommand(Guid ExternalId) : ICommand;
