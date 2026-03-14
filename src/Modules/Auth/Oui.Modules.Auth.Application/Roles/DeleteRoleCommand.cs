using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Roles;

public sealed record DeleteRoleCommand(Guid ExternalId, string DeletedBy) : ICommand;
