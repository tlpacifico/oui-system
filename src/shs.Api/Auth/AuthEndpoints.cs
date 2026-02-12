using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            [FromServices] ShsDbContext db,
            [FromServices] IConfiguration config,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { error = "Email and password are required." });

            var user = await db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email.Trim(), ct);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = BuildJwt(user, config);
            var expiresAt = DateTime.UtcNow.AddMinutes(GetExpirationMinutes(config));
            var userInfo = new UserInfo(user.ExternalId, user.Email, user.DisplayName ?? user.Email);
            return Results.Ok(new LoginResponse(token, expiresAt, userInfo));
        });
    }

    private static string BuildJwt(UserEntity user, IConfiguration config)
    {
        var secret = config["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not set.");
        var issuer = config["Jwt:Issuer"] ?? "OUI-System";
        var audience = config["Jwt:Audience"] ?? "OUI-System";
        var expirationMinutes = GetExpirationMinutes(config);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.ExternalId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("display_name", user.DisplayName ?? user.Email)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static int GetExpirationMinutes(IConfiguration config)
    {
        return int.TryParse(config["Jwt:ExpirationMinutes"], out var m) && m > 0 ? m : 60;
    }
}

public record LoginRequest(string Email, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt, UserInfo User);

public record UserInfo(Guid Id, string Email, string DisplayName);
