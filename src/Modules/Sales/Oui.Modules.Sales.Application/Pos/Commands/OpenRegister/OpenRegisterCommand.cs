using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Commands.OpenRegister;

public sealed record OpenRegisterCommand(
    decimal OpeningAmount,
    string UserId,
    string UserName) : ICommand<RegisterResponse>;
