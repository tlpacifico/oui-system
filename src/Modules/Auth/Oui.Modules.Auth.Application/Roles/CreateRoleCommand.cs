using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Roles;

public sealed record CreateRoleCommand(string Name, string? Description, string CreatedBy) : ICommand<Guid>;
