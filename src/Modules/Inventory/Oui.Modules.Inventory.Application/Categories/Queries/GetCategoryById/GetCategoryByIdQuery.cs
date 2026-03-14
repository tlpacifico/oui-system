using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid ExternalId) : IQuery<CategoryDetailResponse>;
