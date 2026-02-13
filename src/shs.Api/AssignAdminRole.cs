using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Api;

/// <summary>
/// Utility class to assign Admin role to a specific user.
/// Can be called from Program.cs during startup or as a one-time setup.
/// </summary>
public static class AssignAdminRole
{
    public static async Task AssignAdminToUserAsync(
        ShsDbContext db,
        string email,
        string? firebaseUserId = null)
    {
        try
        {
            // Find or create user
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                Console.WriteLine($"Creating user: {email}");

                // Generate a deterministic GUID from Firebase UID or create new one
                Guid externalId;
                if (!string.IsNullOrEmpty(firebaseUserId) && Guid.TryParse(firebaseUserId, out var parsedGuid))
                {
                    externalId = parsedGuid;
                }
                else
                {
                    // Firebase UIDs are not GUIDs, so generate a new one
                    externalId = Guid.NewGuid();
                    Console.WriteLine($"Generated new GUID: {externalId}");
                }

                user = new UserEntity
                {
                    ExternalId = externalId,
                    Email = email,
                    PasswordHash = string.Empty, // Not used, Firebase handles auth
                    DisplayName = email.Split('@')[0],
                    CreatedOn = DateTime.UtcNow
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
                Console.WriteLine($"✓ User created with ID: {user.Id}, ExternalId: {user.ExternalId}");
            }
            else
            {
                Console.WriteLine($"✓ User already exists: {email} (ID: {user.Id}, ExternalId: {user.ExternalId})");
            }

            // Find Admin role
            var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin" && !r.IsDeleted);

            if (adminRole == null)
            {
                Console.WriteLine("✗ Admin role not found. Make sure seeding has run.");
                return;
            }

            Console.WriteLine($"✓ Admin role found (ID: {adminRole.Id})");

            // Check if user already has Admin role
            var existingAssignment = await db.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == adminRole.Id);

            if (existingAssignment != null)
            {
                Console.WriteLine("✓ User already has Admin role");
                return;
            }

            // Assign Admin role
            var userRole = new UserRoleEntity
            {
                UserId = user.Id,
                RoleId = adminRole.Id,
                AssignedOn = DateTime.UtcNow,
                AssignedBy = "system-setup"
            };

            db.UserRoles.Add(userRole);
            await db.SaveChangesAsync();

            Console.WriteLine($"✓✓✓ SUCCESS! Admin role assigned to {email}");

            // Load and display user's permissions
            var permissions = await db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
                .Distinct()
                .ToListAsync();

            Console.WriteLine($"\nUser now has {permissions.Count} permissions:");
            foreach (var perm in permissions.OrderBy(p => p))
            {
                Console.WriteLine($"  - {perm}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            throw;
        }
    }
}
