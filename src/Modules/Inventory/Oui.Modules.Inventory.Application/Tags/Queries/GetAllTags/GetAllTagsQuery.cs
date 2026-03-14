using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Tags.Queries.GetAllTags;

public sealed record GetAllTagsQuery(string? Search) : IQuery<List<TagListResponse>>;
