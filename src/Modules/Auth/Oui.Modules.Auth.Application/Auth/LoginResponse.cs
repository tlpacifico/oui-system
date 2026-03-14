namespace Oui.Modules.Auth.Application.Auth;

public sealed record LoginResponse(string Token, DateTime ExpiresAt, LoginUserInfo User);

public sealed record LoginUserInfo(Guid Id, string Email, string DisplayName);
