using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Brands.Commands.CreateBrand;

public sealed record CreateBrandCommand(
    string Name,
    string? Description,
    string? LogoUrl,
    string? UserEmail) : ICommand<BrandDetailResponse>;
