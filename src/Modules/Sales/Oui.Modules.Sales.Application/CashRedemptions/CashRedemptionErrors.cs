using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.CashRedemptions;

public static class CashRedemptionErrors
{
    public static readonly Error SupplierNotFound = Error.NotFound(
        "CashRedemption.SupplierNotFound", "Supplier not found.");

    public static readonly Error AmountMustBePositive = Error.Problem(
        "CashRedemption.AmountMustBePositive", "O valor deve ser positivo.");

    public static Error InsufficientBalance(decimal available) => Error.Problem(
        "CashRedemption.InsufficientBalance", $"Saldo insuficiente. Disponível: {available:C}");

    public static readonly Error InvalidConversionRates = Error.Problem(
        "CashRedemption.InvalidConversionRates",
        "O fornecedor não tem percentagens de crédito/resgate válidas configuradas.");
}
