using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Commands.CloseRegister;

public sealed record CloseRegisterCommand(
    Guid RegisterExternalId,
    decimal ClosingAmount,
    string? Notes,
    string UserId) : ICommand<CloseRegisterResponse>;
