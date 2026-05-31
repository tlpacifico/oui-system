using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Colors;

public static class ColorErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Color.NotFound", "Color not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Color.NameAlreadyExists", "A color with this name already exists.");

    public static readonly Error HasItems = Error.Conflict(
        "Color.HasItems", "Cannot delete a color that has items assigned to it. Remove the color from items first.");
}
