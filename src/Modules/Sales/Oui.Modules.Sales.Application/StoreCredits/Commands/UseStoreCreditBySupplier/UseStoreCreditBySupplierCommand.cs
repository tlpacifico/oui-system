using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.UseStoreCreditBySupplier;

public sealed record UseStoreCreditBySupplierCommand(
    long SupplierId,
    decimal Amount,
    long? SaleId,
    string? Notes,
    string? UserEmail) : ICommand<UseStoreCreditBySupplierResponse>;
