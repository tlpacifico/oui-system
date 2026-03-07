# 06 — Template para Criar um Novo Modulo

## Estrutura de projetos

Para um modulo chamado `Pedidos`, crie 5 projetos + 3 de teste:

```
src/Modules/Pedidos/
├── SuaApp.Modules.Pedidos.Domain/
├── SuaApp.Modules.Pedidos.Application/
├── SuaApp.Modules.Pedidos.Infrastructure/
├── SuaApp.Modules.Pedidos.Presentation/
└── SuaApp.Modules.Pedidos.IntegrationEvents/

test/Modules/Pedidos/ (opcional, pode ficar em test/)
├── SuaApp.Modules.Pedidos.UnitTests/
├── SuaApp.Modules.Pedidos.ArchitectureTests/
└── SuaApp.Modules.Pedidos.IntegrationTests/
```

## Dependencias entre projetos do modulo

```
Presentation → Application → Domain
                    ↑
              Infrastructure → Domain
                             → Application

IntegrationEvents → (sem dependencias internas, apenas Common.Application)
```

## 1. Domain

**Dependencia**: apenas `SuaApp.Common.Domain`

```
SuaApp.Modules.Pedidos.Domain/
├── Pedidos/
│   ├── Pedido.cs              # Aggregate root
│   ├── IPedidoRepository.cs   # Interface do repositorio
│   ├── PedidoErrors.cs        # Erros tipados
│   └── Events/
│       ├── PedidoCriadoDomainEvent.cs
│       └── PedidoCanceladoDomainEvent.cs
└── Items/
    ├── ItemPedido.cs           # Entidade filha
    └── ItemPedidoErrors.cs
```

**Domain Event**:
```csharp
public sealed class PedidoCriadoDomainEvent(Guid pedidoId) : DomainEvent
{
    public Guid PedidoId { get; init; } = pedidoId;
}
```

## 2. Application

**Dependencias**: `Common.Application`, `Pedidos.Domain`

```
SuaApp.Modules.Pedidos.Application/
├── Abstractions/
│   └── Data/
│       └── IUnitOfWork.cs
├── Pedidos/
│   ├── Commands/
│   │   ├── CriarPedido/
│   │   │   ├── CriarPedidoCommand.cs
│   │   │   ├── CriarPedidoCommandHandler.cs
│   │   │   └── CriarPedidoCommandValidator.cs
│   │   └── CancelarPedido/
│   │       └── ...
│   ├── Queries/
│   │   └── ObterPedido/
│   │       ├── ObterPedidoQuery.cs
│   │       ├── ObterPedidoQueryHandler.cs
│   │       └── PedidoResponse.cs
│   └── DomainEvents/
│       └── PedidoCriadoDomainEventHandler.cs
└── AssemblyReference.cs
```

**AssemblyReference** (necessario para MediatR e Quartz encontrarem os tipos):
```csharp
using System.Reflection;

namespace SuaApp.Modules.Pedidos.Application;

public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}
```

**IUnitOfWork** (por modulo):
```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

## 3. Infrastructure

**Dependencias**: `Common.Infrastructure`, `Pedidos.Application`, `Pedidos.Domain`

```
SuaApp.Modules.Pedidos.Infrastructure/
├── Database/
│   ├── PedidosDbContext.cs
│   ├── Schemas.cs
│   └── Migrations/
├── Pedidos/
│   ├── PedidoRepository.cs
│   └── PedidoConfiguration.cs    # EF Core config
├── Outbox/
│   ├── OutboxOptions.cs
│   ├── ConfigureProcessOutboxJob.cs
│   └── ProcessOutboxJob.cs
├── Inbox/
│   ├── InboxOptions.cs
│   ├── ConfigureProcessInboxJob.cs
│   └── ProcessInboxJob.cs
├── IdempotentDomainEventHandler.cs
├── IdempotentIntegrationEventHandler.cs
└── PedidosModule.cs               # Registro DI do modulo
```

**Schemas.cs**:
```csharp
namespace SuaApp.Modules.Pedidos.Infrastructure;

internal static class Schemas
{
    internal const string Pedidos = "pedidos";
}
```

**DbContext**:
```csharp
public sealed class PedidosDbContext(DbContextOptions<PedidosDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Pedido> Pedidos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Pedidos);

        // Outbox/Inbox tables (do Common)
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

        // Configs do modulo
        modelBuilder.ApplyConfiguration(new PedidoConfiguration());
    }
}
```

**PedidosModule.cs** (registro central do modulo):
```csharp
public static class PedidosModule
{
    public static IServiceCollection AddPedidosModule(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDomainEventHandlers();
        services.AddIntegrationEventHandlers();
        services.AddInfrastructure(configuration);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
        return services;
    }

    public static void ConfigureConsumers(IRegistrationConfigurator registration)
    {
        // Registrar consumers do MassTransit aqui se necessario
    }

    private static void AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PedidosDbContext>((sp, options) =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("Database"),
                    npgsqlOptions => npgsqlOptions
                        .MigrationsHistoryTable(
                            HistoryRepository.DefaultTableName, Schemas.Pedidos))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<InsertOutboxMessagesInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<PedidosDbContext>());

        services.AddScoped<IPedidoRepository, PedidoRepository>();

        services.Configure<OutboxOptions>(
            configuration.GetSection("Pedidos:Outbox"));
        services.ConfigureOptions<ConfigureProcessOutboxJob>();

        services.Configure<InboxOptions>(
            configuration.GetSection("Pedidos:Inbox"));
        services.ConfigureOptions<ConfigureProcessInboxJob>();
    }

    private static void AddDomainEventHandlers(this IServiceCollection services)
    {
        Type[] domainEventHandlers = Application.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler)))
            .ToArray();

        foreach (Type domainEventHandler in domainEventHandlers)
        {
            services.TryAddScoped(domainEventHandler);

            Type domainEvent = domainEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentDomainEventHandler<>).MakeGenericType(domainEvent);

            services.Decorate(domainEventHandler, closedIdempotentHandler);
        }
    }

    private static void AddIntegrationEventHandlers(this IServiceCollection services)
    {
        Type[] integrationEventHandlers = Presentation.AssemblyReference.Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler)))
            .ToArray();

        foreach (Type integrationEventHandler in integrationEventHandlers)
        {
            services.TryAddScoped(integrationEventHandler);

            Type integrationEvent = integrationEventHandler
                .GetInterfaces()
                .Single(i => i.IsGenericType)
                .GetGenericArguments()
                .Single();

            Type closedIdempotentHandler =
                typeof(IdempotentIntegrationEventHandler<>)
                    .MakeGenericType(integrationEvent);

            services.Decorate(integrationEventHandler, closedIdempotentHandler);
        }
    }
}
```

## 4. Presentation

**Dependencias**: `Common.Presentation`, `Pedidos.Application`

```
SuaApp.Modules.Pedidos.Presentation/
├── Pedidos/
│   ├── GetPedido.cs
│   ├── CreatePedido.cs
│   └── CancelPedido.cs
├── Permissions.cs
├── Tags.cs
└── AssemblyReference.cs
```

**Permissions.cs**:
```csharp
internal static class Permissions
{
    internal const string GetPedidos = "pedidos:read";
    internal const string CriarPedidos = "pedidos:create";
    internal const string CancelarPedidos = "pedidos:cancel";
}
```

**Tags.cs** (para Swagger):
```csharp
internal static class Tags
{
    internal const string Pedidos = "Pedidos";
}
```

## 5. IntegrationEvents

**Dependencias**: apenas `Common.Application` (IIntegrationEvent)

```
SuaApp.Modules.Pedidos.IntegrationEvents/
├── PedidoCriadoIntegrationEvent.cs
└── PedidoCanceladoIntegrationEvent.cs
```

Este projeto e referenciado por **outros modulos** que precisam reagir a eventos do modulo Pedidos. E o unico ponto de acoplamento permitido.

```csharp
public sealed class PedidoCriadoIntegrationEvent(
    Guid id, DateTime occurredOnUtc, Guid pedidoId)
    : IntegrationEvent(id, occurredOnUtc)
{
    public Guid PedidoId { get; init; } = pedidoId;
}
```

## 6. Registro no Program.cs

```csharp
// Em Program.cs, adicionar:

// 1. Assembly reference para MediatR
Assembly[] moduleApplicationAssemblies = [
    // ... outros modulos
    SuaApp.Modules.Pedidos.Application.AssemblyReference.Assembly
];

// 2. Consumers do MassTransit
builder.Services.AddInfrastructure(
    serviceName,
    [
        // ... outros modulos
        PedidosModule.ConfigureConsumers
    ],
    databaseConnectionString,
    redisConnectionString);

// 3. Configuraçao do modulo
builder.Configuration.AddModuleConfiguration([
    // ... outros modulos
    "pedidos"
]);

// 4. Registro do modulo
builder.Services.AddPedidosModule(builder.Configuration);
```

## 7. Configuraçao do modulo (modules.pedidos.json)

```json
{
  "Pedidos": {
    "Outbox": {
      "IntervalInSeconds": 5,
      "BatchSize": 50
    },
    "Inbox": {
      "IntervalInSeconds": 5,
      "BatchSize": 50
    }
  }
}
```

E o arquivo de development (`modules.pedidos.Development.json`):
```json
{
  "Pedidos": {
    "Outbox": {
      "IntervalInSeconds": 5,
      "BatchSize": 20
    },
    "Inbox": {
      "IntervalInSeconds": 5,
      "BatchSize": 20
    }
  }
}
```
