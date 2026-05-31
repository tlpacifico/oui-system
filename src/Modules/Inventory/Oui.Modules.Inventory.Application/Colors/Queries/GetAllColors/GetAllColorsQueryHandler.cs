using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors.Queries.GetAllColors;

internal sealed class GetAllColorsQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetAllColorsQuery, List<ColorListResponse>>
{
    public async Task<Result<List<ColorListResponse>>> Handle(
        GetAllColorsQuery request, CancellationToken cancellationToken)
    {
        var query = db.Colors.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s));
        }

        var colors = await query
            .OrderBy(c => c.Name)
            .Select(c => new ColorListResponse(
                c.ExternalId,
                c.Name,
                c.HexCode,
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn))
            .ToListAsync(cancellationToken);

        return colors;
    }
}
