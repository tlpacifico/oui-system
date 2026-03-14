using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Tags.Commands.UpdateTag;

public sealed record UpdateTagCommand(Guid ExternalId, string Name, string? Color) : ICommand<TagDetailResponse>;
