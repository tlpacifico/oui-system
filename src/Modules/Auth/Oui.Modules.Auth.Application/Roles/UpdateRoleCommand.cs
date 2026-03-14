using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Roles;

public sealed record UpdateRoleCommand(Guid ExternalId, string Name, string? Description, string UpdatedBy) : ICommand;
