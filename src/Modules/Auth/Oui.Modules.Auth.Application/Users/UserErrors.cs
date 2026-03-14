using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Users;

public static class UserErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "User.NotFound",
        "Utilizador não encontrado.");
}
