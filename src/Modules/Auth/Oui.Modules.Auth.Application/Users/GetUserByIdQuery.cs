using shs.Application.Messaging;

namespace Oui.Modules.Auth.Application.Users;

public sealed record GetUserByIdQuery(Guid ExternalId) : IQuery<UserDetailResponse>;
