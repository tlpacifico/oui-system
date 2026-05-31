using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Colors.Commands.CreateColor;

public sealed record CreateColorCommand(string Name, string? HexCode) : ICommand<ColorDetailResponse>;
