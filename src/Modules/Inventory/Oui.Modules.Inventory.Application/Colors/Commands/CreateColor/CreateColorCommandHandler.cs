using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors.Commands.CreateColor;

internal sealed class CreateColorCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateColorCommand, ColorDetailResponse>
{
    public async Task<Result<ColorDetailResponse>> Handle(
        CreateColorCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await db.Colors
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
            return Result.Failure<ColorDetailResponse>(ColorErrors.NameAlreadyExists);

        var color = new ColorEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            HexCode = request.HexCode?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Colors.Add(color);
        await db.SaveChangesAsync(cancellationToken);

        return new ColorDetailResponse(
            color.ExternalId,
            color.Name,
            color.HexCode,
            0,
            color.CreatedOn,
            color.CreatedBy,
            color.UpdatedOn,
            color.UpdatedBy);
    }
}
