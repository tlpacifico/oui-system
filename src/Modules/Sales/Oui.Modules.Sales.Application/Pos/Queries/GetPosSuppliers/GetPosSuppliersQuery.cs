using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetPosSuppliers;

public sealed record GetPosSuppliersQuery(string? Search) : IQuery<List<PosSupplierResponse>>;
