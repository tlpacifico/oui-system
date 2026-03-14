using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers;

public static class SupplierErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Supplier.NotFound", "Fornecedor não encontrado.");

    public static readonly Error InitialAlreadyExists = Error.Conflict(
        "Supplier.InitialAlreadyExists", "Já existe um fornecedor com esta inicial.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "Supplier.EmailAlreadyExists", "Já existe um fornecedor com este email.");

    public static readonly Error NifAlreadyExists = Error.Conflict(
        "Supplier.NifAlreadyExists", "Já existe um fornecedor com este NIF.");

    public static readonly Error HasItems = Error.Conflict(
        "Supplier.HasItems", "Não é possível eliminar um fornecedor com itens associados.");

    public static readonly Error HasReceptions = Error.Conflict(
        "Supplier.HasReceptions", "Não é possível eliminar um fornecedor com recepções associadas.");
}
