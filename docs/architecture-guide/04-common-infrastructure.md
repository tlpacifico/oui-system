# 04 — Common.Infrastructure

## Registro central

```csharp
public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        Action<IRegistrationConfigurator>[] moduleConfigureConsumers,
        string databaseConnectionString,
        string redisConnectionString)
    {
        // Auth
        services.AddAuthenticationInternal();
        services.AddAuthorizationInternal();

        // Clock
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        // Event Bus (MassTransit)
        services.TryAddSingleton<IEventBus, EventBus>();

        // Outbox interceptor (EF Core)
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();

        // Database (Npgsql + Dapper)
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(
            databaseConnectionString).Build();
        services.TryAddSingleton(npgsqlDataSource);
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();
        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        // Quartz (background jobs)
        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });
        services.AddQuartzHostedService(options =>
            options.WaitForJobsToComplete = true);

        // Redis cache (fallback para in-memory se indisponivel)
        try
        {
            IConnectionMultiplexer connectionMultiplexer =
                ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory =
                    () => Task.FromResult(connectionMultiplexer));
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }
        services.TryAddSingleton<ICacheService, CacheService>();

        // MassTransit (in-memory bus para modular monolith)
        services.AddMassTransit(configure =>
        {
            foreach (var configureConsumers in moduleConfigureConsumers)
                configureConsumers(configure);

            configure.SetKebabCaseEndpointNameFormatter();
            configure.UsingInMemory((context, cfg) =>
                cfg.ConfigureEndpoints(context));
        });

        // OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql()
                    .AddSource(MassTransit.Logging.DiagnosticHeaders.DefaultListenerName);
                tracing.AddOtlpExporter();
            });

        return services;
    }
}
```

---

## Transactional Outbox Pattern

### Como funciona

1. Entidade de dominio chama `Raise(new AlgoDomainEvent(...))`
2. No `SaveChangesAsync()`, o interceptor captura os domain events e os grava como `OutboxMessage` na **mesma transaçao**
3. Um job do Quartz (`ProcessOutboxJob`) roda periodicamente, le as mensagens nao processadas e despacha para os handlers

### InsertOutboxMessagesInterceptor

```csharp
public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            InsertOutboxMessages(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext context)
    {
        var outboxMessages = context
            .ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                IReadOnlyCollection<IDomainEvent> domainEvents = entity.DomainEvents;
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                Type = domainEvent.GetType().Name,
                Content = JsonConvert.SerializeObject(
                    domainEvent, SerializerSettings.Instance),
                OccurredOnUtc = domainEvent.OccurredOnUtc
            })
            .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
```

### OutboxMessage (entidade + configuraçao EF)

```csharp
public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public string Type { get; init; }
    public string Content { get; init; }       // jsonb
    public DateTime OccurredOnUtc { get; init; }
    public DateTime? ProcessedOnUtc { get; init; }
    public string? Error { get; init; }
}

// EF Config
public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Content).HasMaxLength(2000).HasColumnType("jsonb");
    }
}
```

### ProcessOutboxJob (cada modulo tem o seu)

```csharp
[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
    private const string ModuleName = "MeuModulo"; // ajustar por modulo

    public async Task Execute(IJobExecutionContext context)
    {
        await using DbConnection connection =
            await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction =
            await connection.BeginTransactionAsync();

        // Le mensagens nao processadas com FOR UPDATE (lock)
        IReadOnlyList<OutboxMessageResponse> outboxMessages =
            await GetOutboxMessagesAsync(connection, transaction);

        foreach (OutboxMessageResponse outboxMessage in outboxMessages)
        {
            Exception? exception = null;
            try
            {
                IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
                    outboxMessage.Content, SerializerSettings.Instance)!;

                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IEnumerable<IDomainEventHandler> handlers =
                    DomainEventHandlersFactory.GetHandlers(
                        domainEvent.GetType(),
                        scope.ServiceProvider,
                        Application.AssemblyReference.Assembly);

                foreach (IDomainEventHandler handler in handlers)
                    await handler.Handle(domainEvent, context.CancellationToken);
            }
            catch (Exception caughtException)
            {
                logger.LogError(caughtException,
                    "{Module} - Exception processing outbox message {MessageId}",
                    ModuleName, outboxMessage.Id);
                exception = caughtException;
            }

            await UpdateOutboxMessageAsync(
                connection, transaction, outboxMessage, exception);
        }

        await transaction.CommitAsync();
    }
}
```

### Configuraçao do Job (Quartz)

```csharp
public sealed class OutboxOptions
{
    public int IntervalInSeconds { get; init; }
    public int BatchSize { get; init; }
}

// No appsettings do modulo (modules.meumodulo.json):
// "MeuModulo": { "Outbox": { "IntervalInSeconds": 5, "BatchSize": 50 } }

internal sealed class ConfigureProcessOutboxJob(IOptions<OutboxOptions> options)
    : IConfigureOptions<QuartzOptions>
{
    public void Configure(QuartzOptions quartzOptions)
    {
        string jobName = typeof(ProcessOutboxJob).FullName!;

        quartzOptions
            .AddJob<ProcessOutboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(
                            options.Value.IntervalInSeconds).RepeatForever()));
    }
}
```

---

## Transactional Inbox Pattern

Mesmo conceito do Outbox, mas para **integration events recebidos**. Previne processamento duplicado.

As entidades (`InboxMessage`, `InboxMessageConsumer`) e o job (`ProcessInboxJob`) seguem a mesma estrutura do Outbox, mas usam `IntegrationEventHandlersFactory` e leem da tabela `inbox_messages`.

---

## Idempotent Handlers (via Scrutor Decorator)

Cada domain event handler e integration event handler e decorado automaticamente com um wrapper que verifica se ja foi processado:

```csharp
// Registrado no modulo via:
services.Decorate(domainEventHandler, closedIdempotentHandler);
```

O decorator consulta a tabela `outbox_message_consumers` (ou `inbox_message_consumers`) antes de executar o handler. Se ja existe registro, pula a execuçao.

---

## Autenticaçao e Autorizaçao

### JWT Bearer (Keycloak)

```csharp
internal static IServiceCollection AddAuthenticationInternal(
    this IServiceCollection services)
{
    services.AddAuthorization();
    services.AddAuthentication().AddJwtBearer();
    services.AddHttpContextAccessor();
    services.ConfigureOptions<JwtBearerConfigureOptions>();
    return services;
}
```

### Autorizaçao por Permissions

O sistema cria policies dinamicamente baseado no nome da permission:

```csharp
// No endpoint:
app.MapGet("pedidos/{id}", handler).RequireAuthorization("get:pedidos");

// PermissionAuthorizationPolicyProvider cria a policy automaticamente
// PermissionAuthorizationHandler verifica se o user tem a claim "permission"
// CustomClaimsTransformation injeta as claims de permission no ClaimsPrincipal
```

Fluxo completo:
1. Request chega com JWT
2. `CustomClaimsTransformation` extrai o `identityId` do JWT
3. Chama `IPermissionService.GetUserPermissionsAsync(identityId)`
4. Injeta claims `sub` (userId) e `permission` (cada permissao) no principal
5. `PermissionAuthorizationHandler` verifica se a permission requerida esta nas claims

---

## Cache (Redis)

```csharp
internal sealed class CacheService(IDistributedCache cache) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        byte[]? bytes = await cache.GetAsync(key, ct);
        return bytes is null ? default : JsonSerializer.Deserialize<T>(bytes)!;
    }

    public Task SetAsync<T>(string key, T value,
        TimeSpan? expiration = null, CancellationToken ct = default)
    {
        byte[] bytes = Serialize(value);
        return cache.SetAsync(key, bytes,
            CacheOptions.Create(expiration), ct);
    }

    public Task RemoveAsync(string key, CancellationToken ct = default) =>
        cache.RemoveAsync(key, ct);
}

// Default: 2 minutos de expiraçao
public static class CacheOptions
{
    public static DistributedCacheEntryOptions DefaultExpiration => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };
}
```

---

## Serializaçao (Newtonsoft para Outbox/Inbox)

```csharp
public static class SerializerSettings
{
    public static readonly JsonSerializerSettings Instance = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
    };
}
```

> **Importante**: `TypeNameHandling.All` e necessario para deserializar domain events polimorficos corretamente na hora de processar o outbox.

---

## EventBus (MassTransit)

```csharp
internal sealed class EventBus(IBus bus) : IEventBus
{
    public async Task PublishAsync<T>(
        T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        await bus.Publish(integrationEvent, cancellationToken);
    }
}
```

No modular monolith, o MassTransit usa transporte **in-memory** (`UsingInMemory`). Se migrar para microservicos, basta trocar para RabbitMQ/Azure Service Bus sem alterar o codigo dos modulos.
