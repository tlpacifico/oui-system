# 02 — Common.Domain (Building Blocks do Dominio)

Este projeto contem as abstraçoes fundamentais do DDD. Nao tem dependencias externas.

## Entity (classe base para todas as entidades)

```csharp
namespace SuaApp.Common.Domain;

public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected Entity() { }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

**Uso**: Toda entidade herda de `Entity`. Quando algo relevante acontece no dominio, a entidade chama `Raise(new AlgoAconteceuDomainEvent(...))`. O interceptor do EF Core captura esses eventos na hora do SaveChanges e os grava na tabela outbox.

## IDomainEvent / DomainEvent

```csharp
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOnUtc { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }
    public DateTime OccurredOnUtc { get; init; }
}
```

## Result Pattern

O `Result<T>` substitui exceçoes para erros de negocio. Toda operaçao de dominio retorna `Result` ou `Result<T>`.

```csharp
public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            "The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
```

## Error e ErrorType

```csharp
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4
}

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null", "Null value was provided", ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);
    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);
    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);
    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);
}
```

## ValidationError

```csharp
public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base("General.Validation", "One or more validation errors occurred",
               ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
```

## Exemplo de uso em uma entidade de dominio

```csharp
public sealed class Pedido : Entity
{
    private Pedido() { } // EF Core

    public Guid Id { get; private set; }
    public string Descricao { get; private set; }
    public StatusPedido Status { get; private set; }

    public static Result<Pedido> Criar(string descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            return Result.Failure<Pedido>(PedidoErrors.DescricaoVazia);

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            Descricao = descricao,
            Status = StatusPedido.Criado
        };

        pedido.Raise(new PedidoCriadoDomainEvent(pedido.Id));

        return pedido;
    }

    public Result Cancelar()
    {
        if (Status == StatusPedido.Finalizado)
            return Result.Failure(PedidoErrors.JaFinalizado);

        Status = StatusPedido.Cancelado;
        Raise(new PedidoCanceladoDomainEvent(Id));

        return Result.Success();
    }
}

// Erros tipados por entidade
public static class PedidoErrors
{
    public static readonly Error DescricaoVazia = Error.Problem(
        "Pedido.DescricaoVazia", "A descricao do pedido nao pode ser vazia");

    public static readonly Error JaFinalizado = Error.Problem(
        "Pedido.JaFinalizado", "Pedido ja finalizado nao pode ser cancelado");

    public static Error NaoEncontrado(Guid id) => Error.NotFound(
        "Pedido.NaoEncontrado", $"Pedido com ID '{id}' nao foi encontrado");
}
```
