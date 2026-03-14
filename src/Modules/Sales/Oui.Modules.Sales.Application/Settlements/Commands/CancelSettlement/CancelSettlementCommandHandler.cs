using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Settlements.Commands.CancelSettlement;

internal sealed class CancelSettlementCommandHandler(SalesDbContext salesDb)
    : ICommandHandler<CancelSettlementCommand, CancelSettlementResponse>
{
    public async Task<Result<CancelSettlementResponse>> Handle(
        CancelSettlementCommand request, CancellationToken cancellationToken)
    {
        var settlement = await salesDb.Settlements
            .FirstOrDefaultAsync(s => !s.IsDeleted && s.ExternalId == request.ExternalId, cancellationToken);

        if (settlement == null)
            return Result.Failure<CancelSettlementResponse>(SettlementErrors.NotFound);

        if (settlement.Status == SettlementStatus.Paid)
            return Result.Failure<CancelSettlementResponse>(SettlementErrors.CannotCancelPaid);

        var now = DateTime.UtcNow;

        var saleItems = await salesDb.SaleItems
            .Where(si => si.SettlementId == settlement.Id)
            .ToListAsync(cancellationToken);

        foreach (var saleItem in saleItems)
            saleItem.SettlementId = null;

        settlement.Status = SettlementStatus.Cancelled;
        settlement.UpdatedOn = now;
        settlement.UpdatedBy = request.UserEmail;

        await salesDb.SaveChangesAsync(cancellationToken);

        return new CancelSettlementResponse(
            settlement.ExternalId,
            settlement.Status,
            "Settlement cancelled successfully.");
    }
}
