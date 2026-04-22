using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Users;

public sealed record UpdateUserCommand(Guid ExternalId, string? DisplayName, string UpdatedBy) : ICommand;
