using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors.Commands.UpdateColor;

internal sealed class UpdateColorCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateColorCommand, ColorDetailResponse>
{
    public async Task<Result<ColorDetailResponse>> Handle(
        UpdateColorCommand request, CancellationToken cancellationToken)
    {
        var color = await db.Colors
            .FirstOrDefaultAsync(c => c.ExternalId == request.ExternalId, cancellationToken);

        if (color is null)
            return Result.Failure<ColorDetailResponse>(ColorErrors.NotFound);

        var nameExists = await db.Colors
            .AnyAsync(c => c.Name.ToLower() == request.Name.Trim().ToLower() && c.Id != color.Id, cancellationToken);

        if (nameExists)
            return Result.Failure<ColorDetailResponse>(ColorErrors.NameAlreadyExists);

        color.Name = request.Name.Trim();
        color.HexCode = request.HexCode?.Trim();
        color.UpdatedOn = DateTime.UtcNow;
        color.UpdatedBy = "system";

        await db.SaveChangesAsync(cancellationToken);

        var itemCount = await db.Items
            .CountAsync(i => i.Colors.Any(c => c.Id == color.Id) && !i.IsDeleted, cancellationToken);

        return new ColorDetailResponse(
            color.ExternalId,
            color.Name,
            color.HexCode,
            itemCount,
            color.CreatedOn,
            color.CreatedBy,
            color.UpdatedOn,
            color.UpdatedBy);
    }
}
