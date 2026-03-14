using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UnpublishProduct;

public sealed record UnpublishProductCommand(Guid ExternalId) : ICommand<UnpublishProductResponse>;
