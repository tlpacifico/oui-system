using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.UseStoreCredit;

public sealed record UseStoreCreditCommand(
    Guid ExternalId,
    decimal Amount,
    long? SaleId,
    string? Notes,
    string? UserEmail) : ICommand<UseStoreCreditResponse>;
