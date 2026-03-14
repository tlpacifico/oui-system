using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery(string? Search) : IQuery<List<CategoryListResponse>>;
