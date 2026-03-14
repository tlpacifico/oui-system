using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Users;

public sealed record GetAllUsersQuery(string? Search) : IQuery<List<UserListResponse>>;
