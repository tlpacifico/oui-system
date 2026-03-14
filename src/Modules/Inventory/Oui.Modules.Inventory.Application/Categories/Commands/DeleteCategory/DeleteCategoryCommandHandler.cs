using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories.Commands.DeleteCategory;

internal sealed class DeleteCategoryCommandHandler(InventoryDbContext db)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.ExternalId == request.ExternalId, cancellationToken);

        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        var hasItems = await db.Items.AnyAsync(i => i.CategoryId == category.Id && !i.IsDeleted, cancellationToken);
        if (hasItems)
            return Result.Failure(CategoryErrors.HasItems);

        var hasChildren = await db.Categories.AnyAsync(c => c.ParentCategoryId == category.Id && !c.IsDeleted, cancellationToken);
        if (hasChildren)
            return Result.Failure(CategoryErrors.HasSubCategories);

        category.IsDeleted = true;
        category.DeletedBy = "system";
        category.DeletedOn = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
