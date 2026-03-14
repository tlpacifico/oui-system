using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.StoreCredits;

public static class StoreCreditErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "StoreCredit.NotFound", "Store credit not found.");

    public static readonly Error SupplierNotFound = Error.NotFound(
        "StoreCredit.SupplierNotFound", "Fornecedor não encontrado.");

    public static readonly Error NotActive = Error.Problem(
        "StoreCredit.NotActive", "Store credit is not active.");

    public static readonly Error Expired = Error.Problem(
        "StoreCredit.Expired", "Store credit has expired.");

    public static readonly Error AmountMustBePositive = Error.Problem(
        "StoreCredit.AmountMustBePositive", "Amount must be greater than zero.");

    public static Error InsufficientBalance(decimal available) => Error.Problem(
        "StoreCredit.InsufficientBalance", $"Insufficient balance. Available: {available:C}");

    public static Error InsufficientSupplierBalance(decimal available) => Error.Problem(
        "StoreCredit.InsufficientSupplierBalance", $"Saldo insuficiente. Disponível: {available:C}");

    public static readonly Error NegativeBalanceAfterAdjustment = Error.Problem(
        "StoreCredit.NegativeBalanceAfterAdjustment", "Adjustment would result in negative balance.");

    public static readonly Error AlreadyCancelled = Error.Conflict(
        "StoreCredit.AlreadyCancelled", "Store credit is already cancelled.");
}
