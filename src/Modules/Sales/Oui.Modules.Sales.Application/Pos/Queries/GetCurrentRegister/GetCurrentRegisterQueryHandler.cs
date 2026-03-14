using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetCurrentRegister;

internal sealed class GetCurrentRegisterQueryHandler(SalesDbContext db)
    : IQueryHandler<GetCurrentRegisterQuery, CurrentRegisterResponse>
{
    public async Task<Result<CurrentRegisterResponse>> Handle(
        GetCurrentRegisterQuery request, CancellationToken cancellationToken)
    {
        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
            .FirstOrDefaultAsync(r => r.OperatorUserId == request.UserId
                                      && r.Status == CashRegisterStatus.Open, cancellationToken);

        if (register is null)
            return new CurrentRegisterResponse(false, null);

        var salesCount = register.Sales.Count(s => s.Status == SaleStatus.Active);
        var salesTotal = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .Sum(s => s.TotalAmount);

        return new CurrentRegisterResponse(
            true,
            new RegisterResponse(
                register.ExternalId,
                register.RegisterNumber,
                register.OperatorName,
                register.OpenedAt,
                register.ClosedAt,
                register.OpeningAmount,
                register.ClosingAmount,
                register.ExpectedAmount,
                register.Discrepancy,
                register.Status.ToString(),
                salesCount,
                salesTotal));
    }
}
