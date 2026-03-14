using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Auth;

public static class AuthErrors
{
    public static readonly Error EmailAndPasswordRequired = Error.Problem(
        "Auth.EmailAndPasswordRequired",
        "Email and password are required.");

    public static readonly Error InvalidCredentials = Error.Problem(
        "Auth.InvalidCredentials",
        "Invalid email or password.");
}
