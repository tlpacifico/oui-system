using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugQuery(string Slug) : IQuery<StoreProductDetailResponse>;
