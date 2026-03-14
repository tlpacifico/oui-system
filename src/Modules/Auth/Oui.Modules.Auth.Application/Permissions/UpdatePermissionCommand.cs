using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Permissions;

public sealed record UpdatePermissionCommand(Guid ExternalId, string Name, string? Description) : ICommand;
