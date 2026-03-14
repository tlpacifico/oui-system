using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.CancelStoreCredit;

public sealed record CancelStoreCreditCommand(
    Guid ExternalId,
    string? Reason,
    string? UserEmail) : ICommand<CancelStoreCreditResponse>;
