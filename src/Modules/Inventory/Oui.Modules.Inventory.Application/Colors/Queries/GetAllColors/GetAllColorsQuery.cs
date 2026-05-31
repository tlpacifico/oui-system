using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Colors.Queries.GetAllColors;

public sealed record GetAllColorsQuery(string? Search) : IQuery<List<ColorListResponse>>;
