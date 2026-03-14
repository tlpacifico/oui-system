using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.StoreCredits.Commands.IssueStoreCredit;

public sealed record IssueStoreCreditCommand(
    long SupplierId,
    decimal Amount,
    DateTime? ExpiresOn,
    string? Notes,
    string? UserEmail) : ICommand<IssueStoreCreditResponse>;
