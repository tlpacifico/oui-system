using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags.Commands.UpdateTag;

internal sealed class UpdateTagCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateTagCommand, TagDetailResponse>
{
    public async Task<Result<TagDetailResponse>> Handle(
        UpdateTagCommand request, CancellationToken cancellationToken)
    {
        var tag = await db.Tags
            .FirstOrDefaultAsync(t => t.ExternalId == request.ExternalId, cancellationToken);

        if (tag is null)
            return Result.Failure<TagDetailResponse>(TagErrors.NotFound);

        var nameExists = await db.Tags
            .AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower() && t.Id != tag.Id, cancellationToken);

        if (nameExists)
            return Result.Failure<TagDetailResponse>(TagErrors.NameAlreadyExists);

        tag.Name = request.Name.Trim();
        tag.Color = request.Color?.Trim();
        tag.UpdatedOn = DateTime.UtcNow;
        tag.UpdatedBy = "system";

        await db.SaveChangesAsync(cancellationToken);

        var itemCount = await db.Items
            .CountAsync(i => i.Tags.Any(t => t.Id == tag.Id) && !i.IsDeleted, cancellationToken);

        return new TagDetailResponse(
            tag.ExternalId,
            tag.Name,
            tag.Color,
            itemCount,
            tag.CreatedOn,
            tag.CreatedBy,
            tag.UpdatedOn,
            tag.UpdatedBy);
    }
}
