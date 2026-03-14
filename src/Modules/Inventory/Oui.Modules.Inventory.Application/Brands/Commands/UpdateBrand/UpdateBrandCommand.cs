using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Brands.Commands.UpdateBrand;

public sealed record UpdateBrandCommand(
    Guid ExternalId,
    string Name,
    string? Description,
    string? LogoUrl,
    string? UserEmail) : ICommand<BrandDetailResponse>;
