using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags.Queries.GetTagById;

internal sealed class GetTagByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetTagByIdQuery, TagDetailResponse>
{
    public async Task<Result<TagDetailResponse>> Handle(
        GetTagByIdQuery request, CancellationToken cancellationToken)
    {
        var tag = await db.Tags
            .Where(t => t.ExternalId == request.ExternalId)
            .Select(t => new TagDetailResponse(
                t.ExternalId,
                t.Name,
                t.Color,
                t.Items.Count(i => !i.IsDeleted),
                t.CreatedOn,
                t.CreatedBy,
                t.UpdatedOn,
                t.UpdatedBy))
            .FirstOrDefaultAsync(cancellationToken);

        return tag is null
            ? Result.Failure<TagDetailResponse>(TagErrors.NotFound)
            : tag;
    }
}
