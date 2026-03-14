using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Category.NotFound", "Category not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Category.NameAlreadyExists", "A category with this name already exists.");

    public static readonly Error ParentNotFound = Error.NotFound(
        "Category.ParentNotFound", "Parent category not found.");

    public static readonly Error CircularReference = Error.Problem(
        "Category.CircularReference", "A category cannot be its own parent.");

    public static readonly Error HasItems = Error.Conflict(
        "Category.HasItems", "Cannot delete a category that has items assigned to it.");

    public static readonly Error HasSubCategories = Error.Conflict(
        "Category.HasSubCategories", "Cannot delete a category that has subcategories.");
}
