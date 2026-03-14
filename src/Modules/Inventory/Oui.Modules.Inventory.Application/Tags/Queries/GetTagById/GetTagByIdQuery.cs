using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Tags.Queries.GetTagById;

public sealed record GetTagByIdQuery(Guid ExternalId) : IQuery<TagDetailResponse>;
