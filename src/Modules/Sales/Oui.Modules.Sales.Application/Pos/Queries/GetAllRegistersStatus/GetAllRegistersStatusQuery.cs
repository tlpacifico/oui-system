using shs.Application.Messaging;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetAllRegistersStatus;

public sealed record GetAllRegistersStatusQuery(int Days = 7) : IQuery<AllRegistersStatusResponse>;
