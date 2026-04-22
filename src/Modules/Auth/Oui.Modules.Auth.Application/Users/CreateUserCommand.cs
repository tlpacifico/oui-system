using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Users;

public sealed record CreateUserCommand(string Email, string Password, string? DisplayName, string CreatedBy) : ICommand<Guid>;
