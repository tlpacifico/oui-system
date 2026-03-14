using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Products;

public static class StoreProductErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StoreProduct.NotFound", "Produto não encontrado.");
}
