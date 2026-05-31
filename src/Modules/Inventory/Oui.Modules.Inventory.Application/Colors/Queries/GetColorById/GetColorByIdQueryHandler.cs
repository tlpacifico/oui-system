using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors.Queries.GetColorById;

internal sealed class GetColorByIdQueryHandler(InventoryDbContext db)
    : IQueryHandler<GetColorByIdQuery, ColorDetailResponse>
{
    public async Task<Result<ColorDetailResponse>> Handle(
        GetColorByIdQuery request, CancellationToken cancellationToken)
    {
        var color = await db.Colors
            .Where(c => c.ExternalId == request.ExternalId)
            .Select(c => new ColorDetailResponse(
                c.ExternalId,
                c.Name,
                c.HexCode,
                c.Items.Count(i => !i.IsDeleted),
                c.CreatedOn,
                c.CreatedBy,
                c.UpdatedOn,
                c.UpdatedBy))
            .FirstOrDefaultAsync(cancellationToken);

        return color is null
            ? Result.Failure<ColorDetailResponse>(ColorErrors.NotFound)
            : color;
    }
}
