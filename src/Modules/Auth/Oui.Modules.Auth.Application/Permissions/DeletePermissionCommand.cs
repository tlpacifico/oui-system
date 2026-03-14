using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Permissions;

public sealed record DeletePermissionCommand(Guid ExternalId) : ICommand;
