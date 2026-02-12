using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using shs.Api.Auth;
using shs.Infrastructure;
using shs.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? new[] { "http://localhost:4200" })
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddOpenApi();
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
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

await SeedAuthUserIfEmpty(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapGet("/", () => Results.Ok("OUI System API is running."));

app.Run();

static async Task SeedAuthUserIfEmpty(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ShsDbContext>();
    if (await db.Users.AnyAsync())
        return;
    var user = new shs.Domain.Entities.UserEntity
    {
        ExternalId = Guid.NewGuid(),
        Email = "admin@oui.local",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
        DisplayName = "Administrator",
        CreatedOn = DateTime.UtcNow
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
}
