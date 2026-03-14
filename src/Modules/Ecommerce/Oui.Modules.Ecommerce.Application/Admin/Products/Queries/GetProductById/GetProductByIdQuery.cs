using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ExternalId) : IQuery<ProductDetailResponse>;
