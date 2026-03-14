using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Commands.CancelSettlement;

public sealed record CancelSettlementCommand(
    Guid ExternalId,
    string? UserEmail) : ICommand<CancelSettlementResponse>;
