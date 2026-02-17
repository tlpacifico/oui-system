using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using shs.Api;
using shs.Api.Admin;
using shs.Api.Auth;
using shs.Api.Authorization;
using shs.Api.Consignment;
using shs.Api.Dashboard;
using shs.Api.Reports;
using shs.Api.Financial;
using shs.Api.Inventory;
using shs.Api.Pos;
using shs.Infrastructure;
using shs.Infrastructure.Database;
using shs.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ??
                new[] { "http://localhost:4200" })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);

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
if (builder.Environment.IsProduction())
{
    builder.Services.AddSpaStaticFiles(spa => { spa.RootPath = "angular-client/dist"; });
}

var app = builder.Build();

// Apply migrations and auto-seed RBAC on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShsDbContext>();
    await db.Database.MigrateAsync();

    var seedService = scope.ServiceProvider.GetRequiredService<RbacSeedService>();
    await seedService.SeedAsync();

    // Assign Admin role to initial user
    await AssignAdminRole.AssignAdminToUserAsync(
        db,
        email: "thacio.pacifico@gmail.com"
    );
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
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
app.MapSupplierReturnEndpoints();
app.MapPosEndpoints();
app.MapSalesEndpoints();
app.MapRoleEndpoints();
app.MapPermissionEndpoints();
app.MapRolePermissionEndpoints();
app.MapUserRoleEndpoints();
app.MapMeEndpoints();
app.MapDashboardEndpoints();
app.MapReportsEndpoints();
app.MapSettlementEndpoints();
app.MapStoreCreditEndpoints();
app.MapCashRedemptionEndpoints();
app.MapFallbackToFile("index.html");

app.Run();