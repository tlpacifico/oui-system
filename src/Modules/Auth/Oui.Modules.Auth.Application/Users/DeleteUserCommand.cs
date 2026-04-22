using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Users;

public sealed record DeleteUserCommand(Guid ExternalId, string RequestedBy) : ICommand;
