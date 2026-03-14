using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetAllRegistersStatus;

internal sealed class GetAllRegistersStatusQueryHandler(SalesDbContext db)
    : IQueryHandler<GetAllRegistersStatusQuery, AllRegistersStatusResponse>
{
    public async Task<Result<AllRegistersStatusResponse>> Handle(
        GetAllRegistersStatusQuery request, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.UtcNow.AddDays(-request.Days);

        var registers = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted && s.Status == SaleStatus.Active))
            .Where(r => r.Status == CashRegisterStatus.Open || r.OpenedAt >= cutoff)
            .OrderByDescending(r => r.OpenedAt)
            .Select(r => new RegisterStatusItem(
                r.ExternalId,
                r.RegisterNumber,
                r.OperatorName,
                r.OpenedAt,
                r.ClosedAt,
                r.Status.ToString(),
                r.Sales.Count(s => s.Status == SaleStatus.Active),
                r.Sales.Where(s => s.Status == SaleStatus.Active).Sum(s => s.TotalAmount),
                r.Discrepancy))
            .ToListAsync(cancellationToken);

        var openCount = registers.Count(r => r.Status == "Open");
        var closedCount = registers.Count(r => r.Status == "Closed");

        return new AllRegistersStatusResponse(openCount, closedCount, registers);
    }
}
