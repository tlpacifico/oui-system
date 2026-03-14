using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Settlements.Commands.ProcessPayment;

public sealed record ProcessPaymentCommand(
    Guid ExternalId,
    string? UserEmail) : ICommand<ProcessPaymentResponse>;
