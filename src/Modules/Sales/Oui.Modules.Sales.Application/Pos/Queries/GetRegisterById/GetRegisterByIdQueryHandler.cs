using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Queries.GetRegisterById;

internal sealed class GetRegisterByIdQueryHandler(SalesDbContext db)
    : IQueryHandler<GetRegisterByIdQuery, RegisterDetailResponse>
{
    public async Task<Result<RegisterDetailResponse>> Handle(
        GetRegisterByIdQuery request, CancellationToken cancellationToken)
    {
        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Payments)
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Items)
            .FirstOrDefaultAsync(r => r.ExternalId == request.ExternalId, cancellationToken);

        if (register is null)
            return Result.Failure<RegisterDetailResponse>(PosErrors.RegisterNotFound);

        var activeSales = register.Sales.Where(s => s.Status == SaleStatus.Active).ToList();
        var salesCount = activeSales.Count;
        var salesTotal = activeSales.Sum(s => s.TotalAmount);
        var itemsCount = activeSales.Sum(s => s.Items.Count);

        var salesByMethod = activeSales
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(p => p.Amount));

        var salesList = register.Sales
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new RegisterSaleInfo(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString()))
            .ToList();

        return new RegisterDetailResponse(
            register.ExternalId,
            register.RegisterNumber,
            register.OperatorUserId,
            register.OperatorName,
            register.OpenedAt,
            register.ClosedAt,
            register.OpeningAmount,
            register.ClosingAmount,
            register.ExpectedAmount,
            register.Discrepancy,
            register.DiscrepancyNotes,
            register.Status.ToString(),
            salesCount,
            salesTotal,
            itemsCount,
            salesByMethod,
            salesList,
            register.CreatedOn);
    }
}
