using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags.Commands.CreateTag;

internal sealed class CreateTagCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateTagCommand, TagDetailResponse>
{
    public async Task<Result<TagDetailResponse>> Handle(
        CreateTagCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await db.Tags
            .AnyAsync(t => t.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);

        if (nameExists)
            return Result.Failure<TagDetailResponse>(TagErrors.NameAlreadyExists);

        var tag = new TagEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Color = request.Color?.Trim(),
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Tags.Add(tag);
        await db.SaveChangesAsync(cancellationToken);

        return new TagDetailResponse(
            tag.ExternalId,
            tag.Name,
            tag.Color,
            0,
            tag.CreatedOn,
            tag.CreatedBy,
            tag.UpdatedOn,
            tag.UpdatedBy);
    }
}
