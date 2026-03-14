using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Commands.CloseRegister;

internal sealed class CloseRegisterCommandHandler(SalesDbContext db)
    : ICommandHandler<CloseRegisterCommand, CloseRegisterResponse>
{
    public async Task<Result<CloseRegisterResponse>> Handle(
        CloseRegisterCommand request, CancellationToken cancellationToken)
    {
        if (request.ClosingAmount < 0)
            return Result.Failure<CloseRegisterResponse>(PosErrors.NegativeClosingAmount);

        var register = await db.CashRegisters
            .Include(r => r.Sales.Where(s => !s.IsDeleted))
                .ThenInclude(s => s.Payments)
            .FirstOrDefaultAsync(r => r.ExternalId == request.RegisterExternalId
                                      && r.Status == CashRegisterStatus.Open, cancellationToken);

        if (register is null)
            return Result.Failure<CloseRegisterResponse>(PosErrors.RegisterAlreadyClosed);

        if (register.OperatorUserId != request.UserId)
            return Result.Failure<CloseRegisterResponse>(PosErrors.RegisterNotOwned);

        var cashSalesTotal = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .SelectMany(s => s.Payments)
            .Where(p => p.PaymentMethod == PaymentMethodType.Cash)
            .Sum(p => p.Amount);

        var expectedAmount = register.OpeningAmount + cashSalesTotal;
        var discrepancy = request.ClosingAmount - expectedAmount;

        var salesByMethod = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Sum(p => p.Amount));

        var salesCount = register.Sales.Count(s => s.Status == SaleStatus.Active);
        var totalRevenue = register.Sales
            .Where(s => s.Status == SaleStatus.Active)
            .Sum(s => s.TotalAmount);

        register.ClosedAt = DateTime.UtcNow;
        register.ClosingAmount = request.ClosingAmount;
        register.ExpectedAmount = expectedAmount;
        register.Discrepancy = discrepancy;
        register.DiscrepancyNotes = request.Notes?.Trim();
        register.Status = CashRegisterStatus.Closed;
        register.UpdatedOn = DateTime.UtcNow;
        register.UpdatedBy = request.UserId;

        await db.SaveChangesAsync(cancellationToken);

        return new CloseRegisterResponse(
            register.ExternalId,
            register.RegisterNumber,
            register.OperatorName,
            register.OpenedAt,
            register.ClosedAt.Value,
            salesCount,
            totalRevenue,
            salesByMethod,
            expectedAmount,
            request.ClosingAmount,
            discrepancy);
    }
}
