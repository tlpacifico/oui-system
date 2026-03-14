using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Queries.GetAllSuppliers;

public sealed record GetAllSuppliersQuery(string? Search) : IQuery<List<SupplierListResponse>>;
