using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetBrands;

public sealed record GetBrandsQuery() : IQuery<List<string>>;
