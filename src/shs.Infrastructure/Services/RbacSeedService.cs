using Microsoft.EntityFrameworkCore;
using shs.Domain.Entities;
using shs.Infrastructure.Database;

namespace shs.Infrastructure.Services;

public class RbacSeedService
{
    private readonly ShsDbContext _db;

    public RbacSeedService(ShsDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync()
    {
        // Check if permissions already exist
        if (await _db.Permissions.AnyAsync())
        {
            await EnsureNewPermissionsAsync();
            return;
        }

        var now = DateTime.UtcNow;

        // Define all permissions
        var permissions = new List<PermissionEntity>
        {
            // Admin Category
            new() { ExternalId = Guid.NewGuid(), Name = "admin.roles.view", Category = "admin", Description = "View roles", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.roles.create", Category = "admin", Description = "Create roles", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.roles.update", Category = "admin", Description = "Update roles", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.roles.delete", Category = "admin", Description = "Delete roles", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.roles.manage-permissions", Category = "admin", Description = "Assign/revoke permissions", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.permissions.view", Category = "admin", Description = "View permissions", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.users.view", Category = "admin", Description = "View users", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.users.manage-roles", Category = "admin", Description = "Assign/revoke user roles", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.settings.view", Category = "admin", Description = "View system settings", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.settings.update", Category = "admin", Description = "Update system settings", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "admin.import.execute", Category = "admin", Description = "Execute data import from Excel", CreatedOn = now },

            // Inventory Category
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.items.view", Category = "inventory", Description = "View items", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.items.create", Category = "inventory", Description = "Create items", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.items.update", Category = "inventory", Description = "Update items", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.items.delete", Category = "inventory", Description = "Delete items", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.brands.manage", Category = "inventory", Description = "Manage brands", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.categories.manage", Category = "inventory", Description = "Manage categories", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.tags.manage", Category = "inventory", Description = "Manage tags", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "inventory.suppliers.manage", Category = "inventory", Description = "Manage suppliers", CreatedOn = now },

            // Consignment Category
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.receptions.view", Category = "consignment", Description = "View receptions", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.receptions.create", Category = "consignment", Description = "Create receptions", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.receptions.evaluate", Category = "consignment", Description = "Evaluate receptions", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.receptions.approve", Category = "consignment", Description = "Approve reception evaluations", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.returns.view", Category = "consignment", Description = "View returns", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "consignment.returns.create", Category = "consignment", Description = "Create returns", CreatedOn = now },

            // POS Category
            new() { ExternalId = Guid.NewGuid(), Name = "pos.register.view", Category = "pos", Description = "View cash register", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "pos.register.open", Category = "pos", Description = "Open cash register", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "pos.register.close", Category = "pos", Description = "Close cash register", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "pos.sales.view", Category = "pos", Description = "View sales", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "pos.sales.create", Category = "pos", Description = "Create sales", CreatedOn = now },

            // Reports Category
            new() { ExternalId = Guid.NewGuid(), Name = "dashboard.view", Category = "dashboard", Description = "View dashboard", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "reports.view", Category = "reports", Description = "View reports", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "reports.export", Category = "reports", Description = "Export reports", CreatedOn = now },

            // Ecommerce Category
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.products.view", Category = "ecommerce", Description = "View ecommerce products", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.products.publish", Category = "ecommerce", Description = "Publish items to ecommerce", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.products.update", Category = "ecommerce", Description = "Update ecommerce products", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.products.unpublish", Category = "ecommerce", Description = "Unpublish ecommerce products", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.orders.view", Category = "ecommerce", Description = "View ecommerce orders", CreatedOn = now },
            new() { ExternalId = Guid.NewGuid(), Name = "ecommerce.orders.manage", Category = "ecommerce", Description = "Manage ecommerce orders", CreatedOn = now },
        };

        _db.Permissions.AddRange(permissions);
        await _db.SaveChangesAsync();

        // Create default roles
        var adminRole = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = "Admin",
            Description = "Full system access",
            IsSystemRole = true,
            CreatedOn = now,
            CreatedBy = "system"
        };

        var managerRole = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = "Manager",
            Description = "Inventory management and reports",
            IsSystemRole = true,
            CreatedOn = now,
            CreatedBy = "system"
        };

        var cashierRole = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = "Cashier",
            Description = "Point of sale operations",
            IsSystemRole = true,
            CreatedOn = now,
            CreatedBy = "system"
        };

        var inventoryClerkRole = new RoleEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = "Inventory Clerk",
            Description = "Inventory and consignment operations",
            IsSystemRole = true,
            CreatedOn = now,
            CreatedBy = "system"
        };

        _db.Roles.AddRange(adminRole, managerRole, cashierRole, inventoryClerkRole);
        await _db.SaveChangesAsync();

        // Assign permissions to Admin (all permissions)
        var adminPermissions = permissions.Select(p => new RolePermissionEntity
        {
            RoleId = adminRole.Id,
            PermissionId = p.Id,
            GrantedOn = now,
            GrantedBy = "system"
        }).ToList();

        // Assign permissions to Manager
        var managerPermissionNames = new[]
        {
            "inventory.items.view", "inventory.items.create", "inventory.items.update", "inventory.items.delete",
            "inventory.brands.manage", "inventory.categories.manage", "inventory.tags.manage", "inventory.suppliers.manage",
            "consignment.receptions.view", "consignment.receptions.evaluate", "consignment.receptions.approve",
            "consignment.returns.view",
            "ecommerce.products.view", "ecommerce.products.publish", "ecommerce.products.update", "ecommerce.products.unpublish",
            "ecommerce.orders.view", "ecommerce.orders.manage",
            "dashboard.view", "reports.view", "reports.export"
        };
        var managerPermissions = permissions
            .Where(p => managerPermissionNames.Contains(p.Name))
            .Select(p => new RolePermissionEntity
            {
                RoleId = managerRole.Id,
                PermissionId = p.Id,
                GrantedOn = now,
                GrantedBy = "system"
            }).ToList();

        // Assign permissions to Cashier
        var cashierPermissionNames = new[]
        {
            "pos.register.view", "pos.register.open", "pos.register.close",
            "pos.sales.view", "pos.sales.create",
            "inventory.items.view",
            "dashboard.view"
        };
        var cashierPermissions = permissions
            .Where(p => cashierPermissionNames.Contains(p.Name))
            .Select(p => new RolePermissionEntity
            {
                RoleId = cashierRole.Id,
                PermissionId = p.Id,
                GrantedOn = now,
                GrantedBy = "system"
            }).ToList();

        // Assign permissions to Inventory Clerk
        var inventoryClerkPermissionNames = new[]
        {
            "inventory.items.view", "inventory.items.create", "inventory.items.update",
            "inventory.brands.manage", "inventory.categories.manage", "inventory.tags.manage", "inventory.suppliers.manage",
            "consignment.receptions.view", "consignment.receptions.create", "consignment.receptions.evaluate", "consignment.receptions.approve",
            "consignment.returns.view", "consignment.returns.create",
            "dashboard.view"
        };
        var inventoryClerkPermissions = permissions
            .Where(p => inventoryClerkPermissionNames.Contains(p.Name))
            .Select(p => new RolePermissionEntity
            {
                RoleId = inventoryClerkRole.Id,
                PermissionId = p.Id,
                GrantedOn = now,
                GrantedBy = "system"
            }).ToList();

        _db.RolePermissions.AddRange(adminPermissions);
        _db.RolePermissions.AddRange(managerPermissions);
        _db.RolePermissions.AddRange(cashierPermissions);
        _db.RolePermissions.AddRange(inventoryClerkPermissions);
        await _db.SaveChangesAsync();
    }

    private async Task EnsureNewPermissionsAsync()
    {
        var now = DateTime.UtcNow;
        var newPermissions = new Dictionary<string, string>
        {
            ["admin.import.execute"] = "Execute data import from Excel",
            ["consignment.receptions.approve"] = "Approve reception evaluations",
            ["ecommerce.products.view"] = "View ecommerce products",
            ["ecommerce.products.publish"] = "Publish items to ecommerce",
            ["ecommerce.products.update"] = "Update ecommerce products",
            ["ecommerce.products.unpublish"] = "Unpublish ecommerce products",
            ["ecommerce.orders.view"] = "View ecommerce orders",
            ["ecommerce.orders.manage"] = "Manage ecommerce orders"
        };

        var existingNames = await _db.Permissions.Select(p => p.Name).ToListAsync();
        var missing = newPermissions.Where(kv => !existingNames.Contains(kv.Key)).ToList();
        if (missing.Count == 0) return;

        var adminRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        foreach (var (name, description) in missing)
        {
            var permission = new PermissionEntity
            {
                ExternalId = Guid.NewGuid(),
                Name = name,
                Category = name.Split('.')[0],
                Description = description,
                CreatedOn = now
            };
            _db.Permissions.Add(permission);
            await _db.SaveChangesAsync();

            if (adminRole != null)
            {
                _db.RolePermissions.Add(new RolePermissionEntity
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id,
                    GrantedOn = now,
                    GrantedBy = "system"
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
