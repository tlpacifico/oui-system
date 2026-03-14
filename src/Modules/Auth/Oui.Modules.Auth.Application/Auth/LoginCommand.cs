using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Auth;

public sealed record LoginCommand(string Email, string Password) : ICommand<LoginResponse>;
