using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(
    Guid ExternalId,
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    string? Notes,
    decimal? CreditPercentageInStore,
    decimal? CashRedemptionPercentage) : ICommand<SupplierDetailResponse>;
