using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Tags.Commands.CreateTag;

public sealed record CreateTagCommand(string Name, string? Color) : ICommand<TagDetailResponse>;
