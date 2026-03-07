# 07 — Docker Compose e Infraestrutura

## docker-compose.yml base

```yaml
version: '3.9'

services:
  suaapp.api:
    image: ${DOCKER_REGISTRY-}suaappapi
    container_name: SuaApp.Api
    build:
      context: .
      dockerfile: src/API/SuaApp.Api/Dockerfile
    ports:
      - 5000:8080
      - 5001:8081

  suaapp.database:
    image: postgres:17
    container_name: SuaApp.Database
    environment:
      - POSTGRES_DB=suaapp
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - ./.containers/db:/var/lib/postgresql/data
    ports:
      - 5432:5432

  suaapp.identity:
    image: quay.io/keycloak/keycloak:26.5.1
    container_name: SuaApp.Identity
    command: start-dev --import-realm
    environment:
      - KC_HEALTH_ENABLED=true
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
    volumes:
      - ./.containers/identity:/opt/keycloak/data
      - ./.files:/opt/keycloak/data/import
    ports:
      - 18080:8080
      - 9000:9000
    user: root

  suaapp.seq:
    image: datalust/seq:2025.2
    container_name: SuaApp.Seq
    environment:
      - ACCEPT_EULA=Y
      - SEQ_FIRSTRUN_NOAUTHENTICATION=true
    ports:
      - 5341:5341
      - 8081:80

  suaapp.redis:
    image: redis:latest
    container_name: SuaApp.Redis
    restart: always
    ports:
      - 6379:6379

  suaapp.jaeger:
    image: jaegertracing/all-in-one:latest
    container_name: SuaApp.Jaeger
    ports:
      - 4317:4317
      - 4318:4318
      - 16686:16686
```

## Serviços e seus propósitos

| Serviço | Porta | Uso |
|---------|-------|-----|
| PostgreSQL 17 | 5432 | Banco principal (um schema por modulo) |
| Redis | 6379 | Cache distribuido + estado de Sagas (MassTransit) |
| Keycloak | 18080 | Identity Provider (OAuth2/OIDC, JWT) |
| Seq | 5341/8081 | Agregaçao de logs (Serilog sink) |
| Jaeger | 4317/16686 | Tracing distribuido (OpenTelemetry) |

## Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(databaseConnectionString)
    .AddRedis(redisConnectionString)
    .AddKeyCloak(keyCloakHealthUrl);

// Endpoint
app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

Pacotes: `AspNetCore.HealthChecks.NpgSql`, `AspNetCore.HealthChecks.Redis`, Health check customizado para Keycloak.

## Observabilidade

### Serilog (logging estruturado)

```csharp
// Program.cs
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));
```

Middleware de contexto para trace IDs:

```csharp
app.UseLogContextTraceLogging();  // Adiciona TraceId/SpanId ao log context
app.UseSerilogRequestLogging();   // Loga cada HTTP request
```

### OpenTelemetry (tracing)

Configurado no `InfrastructureConfiguration` com instrumentaçao para:
- ASP.NET Core
- HttpClient
- EF Core
- Redis
- Npgsql
- MassTransit

Export via OTLP para Jaeger.

## appsettings.json (template base)

```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=suaapp;Username=postgres;Password=postgres",
    "Cache": "localhost:6379"
  },
  "Authentication": {
    "Audience": "account",
    "TokenValidationParameters": {
      "ValidIssuers": [
        "http://suaapp.identity:8080/realms/suaapp",
        "http://localhost:18080/realms/suaapp"
      ]
    },
    "MetadataAddress": "http://localhost:18080/realms/suaapp/.well-known/openid-configuration",
    "RequireHttpsMetadata": false
  },
  "Serilog": {
    "Using": ["Serilog.Sinks.Seq"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

## Migrations

Cada modulo gera suas proprias migrations com schema isolado:

```bash
# Criar migration
dotnet ef migrations add NomeDaMigration \
  --project src/Modules/Pedidos/SuaApp.Modules.Pedidos.Infrastructure \
  --startup-project src/API/SuaApp.Api

# Aplicar (automatico em Development via MigrationExtensions)
```

### MigrationExtensions.cs (auto-apply em dev)

```csharp
internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        ApplyMigration<PedidosDbContext>(scope);
        // ... outros modulos
    }

    private static void ApplyMigration<TDbContext>(IServiceScope scope)
        where TDbContext : DbContext
    {
        using TDbContext context =
            scope.ServiceProvider.GetRequiredService<TDbContext>();
        context.Database.Migrate();
    }
}
```
