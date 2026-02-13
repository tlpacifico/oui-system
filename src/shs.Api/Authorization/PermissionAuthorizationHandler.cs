using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using shs.Infrastructure.Database;

namespace shs.Api.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionAuthorizationHandler(
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory serviceScopeFactory)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Create a scope to resolve the DbContext
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShsDbContext>();

        long userId = 0;

        // Try to get user ID from custom JWT (ClaimTypes.NameIdentifier)
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userIdClaim) && long.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }
        else
        {
            // If not found, try to get email from Firebase token and lookup user
            var emailClaim = context.User.FindFirst(ClaimTypes.Email)?.Value
                ?? context.User.FindFirst("email")?.Value;

            if (!string.IsNullOrEmpty(emailClaim))
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == emailClaim);
                if (user != null)
                {
                    userId = user.Id;
                }
            }
        }

        // If we still don't have a user ID, deny access
        if (userId == 0)
        {
            return;
        }

        // Load user's permissions from database based on their roles
        var hasPermission = await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .AnyAsync(permissionName => permissionName == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
