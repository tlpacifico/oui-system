using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetCurrentRegister;

public sealed record GetCurrentRegisterQuery(string UserId) : IQuery<CurrentRegisterResponse>;
