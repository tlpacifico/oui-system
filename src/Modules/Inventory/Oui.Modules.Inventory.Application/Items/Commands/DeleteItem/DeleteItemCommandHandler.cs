using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items.Commands.DeleteItem;

internal sealed class DeleteItemCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteItemCommand>
{
    public async Task<Result> Handle(
        DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.Items
            .FirstOrDefaultAsync(i => i.ExternalId == request.ExternalId, cancellationToken);

        if (item is null)
            return Result.Failure(ItemErrors.NotFound);

        if (item.Status == ItemStatus.Sold)
            return Result.Failure(ItemErrors.CannotDeleteSoldItem);

        item.IsDeleted = true;
        item.DeletedBy = "system";
        item.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
