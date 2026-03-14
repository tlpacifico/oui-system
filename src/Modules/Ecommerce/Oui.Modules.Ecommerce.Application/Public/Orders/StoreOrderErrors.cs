using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Public.Orders;

public static class StoreOrderErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StoreOrder.NotFound", "Encomenda não encontrada.");

    public static readonly Error CustomerNameRequired = Error.Problem(
        "StoreOrder.CustomerNameRequired", "Nome é obrigatório.");

    public static readonly Error CustomerEmailRequired = Error.Problem(
        "StoreOrder.CustomerEmailRequired", "Email é obrigatório.");

    public static readonly Error NoProductsSelected = Error.Problem(
        "StoreOrder.NoProductsSelected", "Selecione pelo menos um produto.");

    public static readonly Error NoProductsAvailable = Error.Problem(
        "StoreOrder.NoProductsAvailable", "Nenhum dos produtos selecionados está disponível.");
}
