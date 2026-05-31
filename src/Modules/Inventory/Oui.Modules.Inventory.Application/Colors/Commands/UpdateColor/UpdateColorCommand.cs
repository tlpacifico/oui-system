using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Colors.Commands.UpdateColor;

public sealed record UpdateColorCommand(Guid ExternalId, string Name, string? HexCode) : ICommand<ColorDetailResponse>;
