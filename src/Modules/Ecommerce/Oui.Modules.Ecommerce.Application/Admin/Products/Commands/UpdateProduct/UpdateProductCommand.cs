using shs.Application.Messaging;

namespace Oui.Modules.Ecommerce.Application.Admin.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ExternalId,
    string? Title,
    string? Description,
    decimal? Price,
    string? BrandName,
    string? CategoryName,
    string? Size,
    string? Color,
    string? Condition,
    string? Composition) : ICommand<UpdateProductResponse>;
