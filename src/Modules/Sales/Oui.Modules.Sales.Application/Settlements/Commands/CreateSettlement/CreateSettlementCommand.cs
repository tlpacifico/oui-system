using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Commands.CreateSettlement;

public sealed record CreateSettlementCommand(
    long SupplierId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    string? Notes,
    string? UserEmail) : ICommand<CreateSettlementResponse>;
