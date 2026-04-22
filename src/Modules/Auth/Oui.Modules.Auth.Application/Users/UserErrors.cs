using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        "Utilizador não encontrado.");

    public static readonly Error EmailAlreadyExists = Error.Conflict(
        "User.EmailAlreadyExists",
        "Já existe um utilizador com este email.");

    public static readonly Error FirebaseError = Error.Problem(
        "User.FirebaseError",
        "Erro ao comunicar com o Firebase Authentication.");

    public static readonly Error CannotDeleteSelf = Error.Problem(
        "User.CannotDeleteSelf",
        "Não é possível eliminar o próprio utilizador.");
}
