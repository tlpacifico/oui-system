using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.SupplierReturns;

public static class SupplierReturnErrors
{
    public static readonly Error SupplierNotFound = Error.NotFound(
        "SupplierReturn.SupplierNotFound", "Fornecedor não encontrado.");

    public static readonly Error NotFound = Error.NotFound(
        "SupplierReturn.NotFound", "Devolução não encontrada.");

    public static readonly Error NoItemsSelected = Error.Problem(
        "SupplierReturn.NoItemsSelected", "Selecione pelo menos um item para devolver.");

    public static readonly Error NoValidItems = Error.Problem(
        "SupplierReturn.NoValidItems", "Nenhum item válido encontrado.");

    public static Error ItemsNotReturnable(string ids) => Error.Problem(
        "SupplierReturn.ItemsNotReturnable", $"Os seguintes itens não podem ser devolvidos (estado inválido): {ids}");
}
