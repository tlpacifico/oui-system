using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Tags;

public static class TagErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Tag.NotFound", "Tag not found.");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Tag.NameAlreadyExists", "A tag with this name already exists.");

    public static readonly Error HasItems = Error.Conflict(
        "Tag.HasItems", "Cannot delete a tag that has items assigned to it. Remove the tag from items first.");
}
