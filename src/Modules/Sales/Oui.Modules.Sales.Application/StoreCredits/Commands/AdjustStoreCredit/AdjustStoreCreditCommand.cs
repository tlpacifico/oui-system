using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.AdjustStoreCredit;

public sealed record AdjustStoreCreditCommand(
    Guid ExternalId,
    decimal AdjustmentAmount,
    string? Reason,
    string? UserEmail) : ICommand<AdjustStoreCreditResponse>;
