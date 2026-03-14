using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishBatch;

public sealed record PublishBatchCommand(List<Guid> ItemExternalIds) : ICommand<PublishBatchResponse>;
