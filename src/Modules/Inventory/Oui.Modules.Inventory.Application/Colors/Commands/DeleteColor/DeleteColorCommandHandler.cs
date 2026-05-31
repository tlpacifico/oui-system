using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors.Commands.DeleteColor;

internal sealed class DeleteColorCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteColorCommand>
{
    public async Task<Result> Handle(
        DeleteColorCommand request, CancellationToken cancellationToken)
    {
        var color = await db.Colors
            .FirstOrDefaultAsync(c => c.ExternalId == request.ExternalId, cancellationToken);

        if (color is null)
            return Result.Failure(ColorErrors.NotFound);

        var hasItems = await db.Items.AnyAsync(i => i.Colors.Any(c => c.Id == color.Id) && !i.IsDeleted, cancellationToken);
        if (hasItems)
            return Result.Failure(ColorErrors.HasItems);

        color.IsDeleted = true;
        color.DeletedBy = "system";
        color.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
