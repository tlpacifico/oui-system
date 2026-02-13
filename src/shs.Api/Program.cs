using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using shs.Api.Auth;
using shs.Api.Inventory;
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


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapInventoryEndpoints();
app.MapBrandEndpoints();
app.MapGet("/", () => Results.Ok("OUI System API is running."));

app.Run();


