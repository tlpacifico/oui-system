using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using shs.Api;
using shs.Api.Admin;
using shs.Api.Auth;
using shs.Api.Authorization;
using shs.Api.Consignment;
using shs.Api.Dashboard;
using shs.Api.Ecommerce;
using shs.Api.Reports;
using shs.Api.Financial;
using shs.Api.Inventory;
using shs.Api.Pos;
using shs.Infrastructure;
using shs.Infrastructure.Services;
using Oui.Modules.Auth.Infrastructure;
using Oui.Modules.Inventory.Infrastructure;
using Oui.Modules.Sales.Infrastructure;
using Oui.Modules.Ecommerce.Infrastructure;
using Oui.Modules.System.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ??
                new[] { "http://localhost:4200", "http://localhost:3000" })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// Register per-module DbContexts and services
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);
builder.Services.AddSalesModule(builder.Configuration);
builder.Services.AddEcommerceModule(builder.Configuration);
builder.Services.AddSystemModule(builder.Configuration);
builder.Services.AddSharedInfrastructure(builder.Configuration);

var firebaseConfig = builder.Configuration.GetSection("Firebase");
var projectId = firebaseConfig["ProjectId"];
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://securetoken.google.com/{projectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"https://securetoken.google.com/{projectId}",
            ValidateAudience = true,
            ValidAudience = projectId,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5) // Allow 5 min skew for token expiry
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ctx.Exception, "JWT validation failed: {Message}", ctx.Exception?.Message);
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<RbacSeedService>();
builder.Services.AddScoped<SystemSettingSeedService>();
if (builder.Environment.IsProduction())
{
    builder.Services.AddSpaStaticFiles(spa => { spa.RootPath = "angular-client/dist"; });
}

var app = builder.Build();

// Apply migrations for all module DbContexts and auto-seed on startup
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AuthDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<SalesDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<EcommerceDbContext>().Database.MigrateAsync();
    await scope.ServiceProvider.GetRequiredService<SystemDbContext>().Database.MigrateAsync();

    var seedService = scope.ServiceProvider.GetRequiredService<RbacSeedService>();
    await seedService.SeedAsync();

    var settingSeedService = scope.ServiceProvider.GetRequiredService<SystemSettingSeedService>();
    await settingSeedService.SeedAsync();

    // Assign Admin role to initial user
    var authDb = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await AssignAdminRole.AssignAdminToUserAsync(
        authDb,
        email: "thacio.pacifico@gmail.com"
    );
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseCors();
app.UseStaticFiles(); // Serve wwwroot/uploads (item photos)
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapInventoryEndpoints();
app.MapBrandEndpoints();
app.MapCategoryEndpoints();
app.MapTagEndpoints();
app.MapSupplierEndpoints();
app.MapConsignmentEndpoints();
app.MapApprovalEndpoints();
app.MapSupplierReturnEndpoints();
app.MapPosEndpoints();
app.MapSalesEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapRolePermissionEndpoints();
app.MapUserRoleEndpoints();
app.MapUserEndpoints();
app.MapMeEndpoints();
app.MapDashboardEndpoints();
app.MapReportsEndpoints();
app.MapSettlementEndpoints();
app.MapStoreCreditEndpoints();
app.MapCashRedemptionEndpoints();
app.MapSystemSettingEndpoints();
app.MapImportEndpoints();
app.MapEcommerceAdminEndpoints();
app.MapEcommercePublicEndpoints();
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible to integration tests (WebApplicationFactory<Program>)
public partial class Program { }
