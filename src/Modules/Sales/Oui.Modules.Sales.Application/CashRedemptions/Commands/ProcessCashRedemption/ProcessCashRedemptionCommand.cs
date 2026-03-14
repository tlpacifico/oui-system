using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.CashRedemptions.Commands.ProcessCashRedemption;

public sealed record ProcessCashRedemptionCommand(
    long SupplierId,
    decimal Amount,
    string? Notes,
    string? UserEmail) : ICommand<ProcessCashRedemptionResponse>;
