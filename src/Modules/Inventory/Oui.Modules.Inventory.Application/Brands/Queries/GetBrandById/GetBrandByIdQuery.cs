using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Brands.Queries.GetBrandById;

public sealed record GetBrandByIdQuery(Guid ExternalId) : IQuery<BrandDetailResponse>;
