using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos.Commands.OpenRegister;

internal sealed class OpenRegisterCommandHandler(SalesDbContext db)
    : ICommandHandler<OpenRegisterCommand, RegisterResponse>
{
    public async Task<Result<RegisterResponse>> Handle(
        OpenRegisterCommand request, CancellationToken cancellationToken)
    {
        if (request.OpeningAmount < 0)
            return Result.Failure<RegisterResponse>(PosErrors.NegativeOpeningAmount);

        var existingOpen = await db.CashRegisters
            .AnyAsync(r => r.OperatorUserId == request.UserId && r.Status == CashRegisterStatus.Open, cancellationToken);

        if (existingOpen)
            return Result.Failure<RegisterResponse>(PosErrors.RegisterAlreadyOpen);

        var lastNumber = await db.CashRegisters
            .IgnoreQueryFilters()
            .MaxAsync(r => (int?)r.RegisterNumber, cancellationToken) ?? 0;

        var register = new CashRegisterEntity
        {
            ExternalId = Guid.NewGuid(),
            OperatorUserId = request.UserId,
            OperatorName = request.UserName,
            RegisterNumber = lastNumber + 1,
            OpenedAt = DateTime.UtcNow,
            OpeningAmount = request.OpeningAmount,
            Status = CashRegisterStatus.Open,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = request.UserId
        };

        db.CashRegisters.Add(register);
        await db.SaveChangesAsync(cancellationToken);

        return new RegisterResponse(
            register.ExternalId,
            register.RegisterNumber,
            register.OperatorName,
            register.OpenedAt,
            null,
            register.OpeningAmount,
            null,
            null,
            null,
            register.Status.ToString(),
            0,
            0m);
    }
}
