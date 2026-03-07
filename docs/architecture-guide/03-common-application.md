# 03 — Common.Application (CQRS e Pipeline)

## Interfaces CQRS

Todos os commands e queries passam pelo MediatR. As interfaces abaixo tipam o retorno como `Result<T>`, garantindo que todo handler retorna um resultado funcional.

### Commands (escritas)

```csharp
// Marker interface
public interface IBaseCommand;

// Command sem retorno (apenas Result de sucesso/falha)
public interface ICommand : IRequest<Result>, IBaseCommand;

// Command com retorno tipado
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

// Handlers
public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
```

### Queries (leituras)

```csharp
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

public interface IQueryHandler<in TQuery, TResponse>
    : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
```

### Domain Event Handlers

```csharp
public interface IDomainEventHandler<in TDomainEvent> : IDomainEventHandler
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventHandler
{
    Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public abstract Task Handle(
        TDomainEvent domainEvent, CancellationToken cancellationToken = default);

    public Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default) =>
        Handle((TDomainEvent)domainEvent, cancellationToken);
}
```

## Pipeline Behaviors (MediatR)

Executados em ordem para **todo** command/query que passa pelo MediatR.

### 1. ExceptionHandlingPipelineBehavior

Captura exceçoes nao tratadas e as transforma em uma exceçao tipada da aplicaçao.

```csharp
internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception for {RequestName}",
                typeof(TRequest).Name);
            throw new SuaAppException(typeof(TRequest).Name, innerException: exception);
        }
    }
}
```

### 2. RequestLoggingPipelineBehavior

Loga o inicio e fim de cada request, incluindo o modulo de origem e o resultado (sucesso/falha).

```csharp
internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string moduleName = GetModuleName(typeof(TRequest).FullName!);
        string requestName = typeof(TRequest).Name;

        Activity.Current?.SetTag("request.module", moduleName);
        Activity.Current?.SetTag("request.name", requestName);

        using (LogContext.PushProperty("Module", moduleName))
        {
            logger.LogInformation("Processing request {RequestName}", requestName);

            TResponse result = await next();

            if (result.IsSuccess)
                logger.LogInformation("Completed request {RequestName}", requestName);
            else
                using (LogContext.PushProperty("Error", result.Error, true))
                    logger.LogError("Completed request {RequestName} with error", requestName);

            return result;
        }
    }

    private static string GetModuleName(string requestName) => requestName.Split('.')[2];
}
```

> **Nota**: `GetModuleName` extrai o nome do modulo do namespace (ex: `SuaApp.Modules.Pedidos.Application.Commands.CriarPedidoCommand` → `Modules`). Ajuste o indice `[2]` conforme seu namespace.

### 3. ValidationPipelineBehavior

Executa todos os `IValidator<TCommand>` do FluentValidation antes do handler. Se houver erros, retorna `Result.ValidationFailure` sem executar o handler.

```csharp
internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationFailure[] validationFailures = await ValidateAsync(request);

        if (validationFailures.Length == 0)
            return await next();

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            if (failureMethod is not null)
                return (TResponse)failureMethod.Invoke(
                    null, [CreateValidationError(validationFailures)]);
        }
        else if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(
                CreateValidationError(validationFailures));
        }

        throw new ValidationException(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
    {
        if (!validators.Any()) return [];

        var context = new ValidationContext<TRequest>(request);

        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));

        return validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToArray();
    }

    private static ValidationError CreateValidationError(ValidationFailure[] failures) =>
        new(failures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}
```

## Registro no DI

```csharp
public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Assembly[] moduleAssemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(moduleAssemblies);

            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

        return services;
    }
}
```

## Abstraçoes de infraestrutura (interfaces no Application)

Estas interfaces ficam em Application para que Infrastructure as implemente (inversao de dependencia):

```csharp
// Clock
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

// Cache
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

// Database connection (para queries Dapper)
public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}

// Event Bus (integration events entre modulos)
public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}

// Permission service (auth)
public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);
}
```

## Integration Events (contratos entre modulos)

```csharp
public interface IIntegrationEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract class IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}

// Handler base
public abstract class IntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public abstract Task Handle(
        TIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);

    public Task Handle(IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default) =>
        Handle((TIntegrationEvent)integrationEvent, cancellationToken);
}
```

## Exemplo: Command + Handler + Validator

```csharp
// Command (sealed record)
public sealed record CriarPedidoCommand(string Descricao, decimal Valor)
    : ICommand<Guid>;

// Validator (internal sealed)
internal sealed class CriarPedidoCommandValidator
    : AbstractValidator<CriarPedidoCommand>
{
    public CriarPedidoCommandValidator()
    {
        RuleFor(c => c.Descricao).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Valor).GreaterThan(0);
    }
}

// Handler (internal sealed)
internal sealed class CriarPedidoCommandHandler(
    IPedidoRepository pedidoRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CriarPedidoCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CriarPedidoCommand command, CancellationToken cancellationToken)
    {
        Result<Pedido> result = Pedido.Criar(command.Descricao);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        pedidoRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}
```

## Exemplo: Query + Handler (Dapper)

```csharp
// Query
public sealed record ObterPedidoQuery(Guid PedidoId) : IQuery<PedidoResponse>;

// Response DTO
public sealed record PedidoResponse(Guid Id, string Descricao, decimal Valor, string Status);

// Handler com Dapper
internal sealed class ObterPedidoQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<ObterPedidoQuery, PedidoResponse>
{
    public async Task<Result<PedidoResponse>> Handle(
        ObterPedidoQuery query, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            """
            SELECT id, descricao, valor, status
            FROM pedidos.pedidos
            WHERE id = @PedidoId
            """;

        PedidoResponse? pedido = await connection.QuerySingleOrDefaultAsync<PedidoResponse>(
            sql, new { query.PedidoId });

        if (pedido is null)
            return Result.Failure<PedidoResponse>(PedidoErrors.NaoEncontrado(query.PedidoId));

        return pedido;
    }
}
```
