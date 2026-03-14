using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Permissions;

public sealed record CreatePermissionCommand(string Name, string? Description) : ICommand<Guid>;
