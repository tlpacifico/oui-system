using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Approvals;

public static class ApprovalErrors
{
    public static readonly Error InvalidToken = Error.NotFound(
        "Approval.InvalidToken", "Link de aprovação inválido.");

    public static readonly Error AlreadyProcessed = Error.Conflict(
        "Approval.AlreadyProcessed", "Esta aprovação já foi processada.");

    public static readonly Error TokenExpired = Error.Problem(
        "Approval.TokenExpired", "Este link de aprovação expirou. Contacte a loja para obter um novo link.");

    public static readonly Error ReceptionNotFound = Error.NotFound(
        "Approval.ReceptionNotFound", "Recepção não encontrada.");

    public static readonly Error NotInEvaluatedState = Error.Conflict(
        "Approval.NotInEvaluatedState", "Esta recepção não está no estado de avaliação concluída.");

    public static readonly Error NoAwaitingItems = Error.Conflict(
        "Approval.NoAwaitingItems", "Não existem itens a aguardar aprovação nesta recepção.");
}
