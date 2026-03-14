using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Me;

public static class MeErrors
{
    public static readonly Error Unauthorized = Error.Problem(
        "Me.Unauthorized",
        "User is not authenticated.");
}
