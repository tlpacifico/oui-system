using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Colors.Queries.GetColorById;

public sealed record GetColorByIdQuery(Guid ExternalId) : IQuery<ColorDetailResponse>;
