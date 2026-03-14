using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Public.Products.Queries.GetCategories;

public sealed record GetCategoriesQuery() : IQuery<List<string>>;
