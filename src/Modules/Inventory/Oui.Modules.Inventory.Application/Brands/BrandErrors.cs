using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Brands;

public static class BrandErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Brand.NotFound", "Brand not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Brand.NameAlreadyExists", "A brand with this name already exists.");

    public static readonly Error HasItems = Error.Conflict(
        "Brand.HasItems", "Cannot delete a brand that has items assigned to it.");
}
