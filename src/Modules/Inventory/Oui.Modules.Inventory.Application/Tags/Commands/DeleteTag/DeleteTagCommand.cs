using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Tags.Commands.DeleteTag;

public sealed record DeleteTagCommand(Guid ExternalId) : ICommand;
