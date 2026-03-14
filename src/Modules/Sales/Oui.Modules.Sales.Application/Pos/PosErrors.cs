using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Pos;

public static class PosErrors
{
    public static readonly Error RegisterNotFound = Error.NotFound(
        "Pos.RegisterNotFound", "Caixa não encontrada.");

    public static readonly Error RegisterAlreadyOpen = Error.Conflict(
        "Pos.RegisterAlreadyOpen", "Já tem uma caixa aberta. Feche a caixa atual antes de abrir outra.");

    public static readonly Error RegisterNotOpen = Error.Problem(
        "Pos.RegisterNotOpen", "Caixa não encontrada ou não está aberta.");

    public static readonly Error RegisterNotOwned = Error.Problem(
        "Pos.RegisterNotOwned", "Não tem permissão para operar esta caixa.");

    public static readonly Error NegativeOpeningAmount = Error.Problem(
        "Pos.NegativeOpeningAmount", "O valor de abertura não pode ser negativo.");

    public static readonly Error NegativeClosingAmount = Error.Problem(
        "Pos.NegativeClosingAmount", "O valor de fecho não pode ser negativo.");

    public static readonly Error RegisterAlreadyClosed = Error.Conflict(
        "Pos.RegisterAlreadyClosed", "Caixa não encontrada ou já está fechada.");

    public static readonly Error NoItems = Error.Problem(
        "Pos.NoItems", "A venda deve ter pelo menos um item.");

    public static readonly Error NoPayments = Error.Problem(
        "Pos.NoPayments", "A venda deve ter pelo menos um pagamento.");

    public static readonly Error TooManyPayments = Error.Problem(
        "Pos.TooManyPayments", "Máximo de 2 métodos de pagamento por venda.");

    public static readonly Error InvalidDiscountPercentage = Error.Problem(
        "Pos.InvalidDiscountPercentage", "A percentagem de desconto deve estar entre 0 e 100.");

    public static readonly Error NegativeTotalAmount = Error.Problem(
        "Pos.NegativeTotalAmount", "O valor total da venda não pode ser negativo.");

    public static Error ItemsNotFound(string ids) => Error.Problem(
        "Pos.ItemsNotFound", $"Item(ns) não encontrado(s): {ids}");

    public static Error ItemsNotSellable(string ids) => Error.Problem(
        "Pos.ItemsNotSellable", $"Os seguintes itens não estão disponíveis para venda: {ids}");

    public static Error NegativeItemDiscount(string itemId) => Error.Problem(
        "Pos.NegativeItemDiscount", $"O desconto do item {itemId} não pode ser negativo.");

    public static Error ItemDiscountExceedsPrice(string itemId) => Error.Problem(
        "Pos.ItemDiscountExceedsPrice", $"O desconto do item {itemId} não pode ser maior que o preço.");

    public static Error InsufficientPayment(decimal paymentTotal, decimal totalAmount) => Error.Problem(
        "Pos.InsufficientPayment", $"O total dos pagamentos ({paymentTotal:F2}€) é inferior ao valor da venda ({totalAmount:F2}€).");

    public static readonly Error NonPositivePayment = Error.Problem(
        "Pos.NonPositivePayment", "O valor de cada pagamento deve ser positivo.");

    public static Error InvalidPaymentMethod(string method) => Error.Problem(
        "Pos.InvalidPaymentMethod", $"Método de pagamento inválido: {method}");

    public static readonly Error StoreCreditRequiresSupplier = Error.Problem(
        "Pos.StoreCreditRequiresSupplier", "Ao usar Crédito em Loja, deve identificar o fornecedor (SupplierId).");

    public static Error InsufficientStoreCredit(string supplierName, decimal available) => Error.Problem(
        "Pos.InsufficientStoreCredit", $"Crédito insuficiente para o fornecedor {supplierName}. Disponível: {available:C}");
}
