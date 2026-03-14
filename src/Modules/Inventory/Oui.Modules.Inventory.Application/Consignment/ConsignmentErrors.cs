using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Consignment;

public static class ConsignmentErrors
{
    public static readonly Error SupplierNotFound = Error.NotFound(
        "Consignment.SupplierNotFound", "Fornecedor não encontrado.");

    public static readonly Error ReceptionNotFound = Error.NotFound(
        "Consignment.ReceptionNotFound", "Recepção não encontrada.");

    public static readonly Error AlreadyEvaluated = Error.Conflict(
        "Consignment.AlreadyEvaluated", "Esta recepção já foi avaliada.");

    public static readonly Error EvaluationNotCompleted = Error.Problem(
        "Consignment.EvaluationNotCompleted", "A avaliação ainda não foi concluída.");

    public static readonly Error NoItemsEvaluated = Error.Problem(
        "Consignment.NoItemsEvaluated", "Não é possível concluir a avaliação sem nenhum item avaliado.");

    public static Error ItemCountMismatch(int remaining, int total) => Error.Problem(
        "Consignment.ItemCountMismatch", $"Faltam avaliar {remaining} de {total} itens.");

    public static Error ItemLimitReached(int current, int max) => Error.Conflict(
        "Consignment.ItemLimitReached", $"Já foram avaliados {current} de {max} itens. Não é possível adicionar mais.");

    public static readonly Error ItemNotFoundInReception = Error.NotFound(
        "Consignment.ItemNotFoundInReception", "Item não encontrado nesta recepção.");

    public static readonly Error CannotRemoveFromEvaluated = Error.Conflict(
        "Consignment.CannotRemoveFromEvaluated", "Não é possível remover itens de uma recepção já avaliada.");

    public static readonly Error BrandNotFound = Error.NotFound(
        "Consignment.BrandNotFound", "Marca não encontrada.");

    public static readonly Error CategoryNotFound = Error.NotFound(
        "Consignment.CategoryNotFound", "Categoria não encontrada.");

    public static readonly Error InvalidCondition = Error.Problem(
        "Consignment.InvalidCondition", "Condição inválida.");

    public static readonly Error EmailSendFailed = Error.Problem(
        "Consignment.EmailSendFailed", "Erro ao enviar email.");
}
