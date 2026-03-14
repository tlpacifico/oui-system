using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Brands.Queries.GetAllBrands;

public sealed record GetAllBrandsQuery(string? Search) : IQuery<List<BrandListResponse>>;
