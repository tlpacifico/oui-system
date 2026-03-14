using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.PublishItem;

public sealed record PublishItemCommand(Guid ItemExternalId) : ICommand<PublishItemResponse>;
