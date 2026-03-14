using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags.Commands.DeleteTag;

internal sealed class DeleteTagCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteTagCommand>
{
    public async Task<Result> Handle(
        DeleteTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await db.Tags
            .FirstOrDefaultAsync(t => t.ExternalId == request.ExternalId, cancellationToken);

        if (tag is null)
            return Result.Failure(TagErrors.NotFound);

        var hasItems = await db.Items.AnyAsync(i => i.Tags.Any(t => t.Id == tag.Id) && !i.IsDeleted, cancellationToken);
        if (hasItems)
            return Result.Failure(TagErrors.HasItems);

        tag.IsDeleted = true;
        tag.DeletedBy = "system";
        tag.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
