using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags.Queries.GetAllTags;

internal sealed class GetAllTagsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetAllTagsQuery, List<TagListResponse>>
{
    public async Task<Result<List<TagListResponse>>> Handle(
        GetAllTagsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(s));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Select(t => new TagListResponse(
                t.ExternalId,
                t.Name,
                t.Color,
                t.Items.Count(i => !i.IsDeleted),
                t.CreatedOn))
            .ToListAsync(cancellationToken);

        return tags;
    }
}
