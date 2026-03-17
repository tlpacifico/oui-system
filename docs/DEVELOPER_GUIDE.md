# Developer Guide

Guia de referência para desenvolvimento de software com foco em **Clean Code**, **SOLID**, **DDD** e **CQRS**.
Todos os exemplos são extraídos do codebase real do projeto Evently.

---

## Índice

1. [Princípios Clean Code](#1-princípios-clean-code)
2. [Guard Clauses](#2-guard-clauses)
3. [Princípios SOLID](#3-princípios-solid)
4. [Result Pattern](#4-result-pattern)
5. [CQRS Pattern](#5-cqrs-pattern)
6. [Domain-Driven Design](#6-domain-driven-design)
7. [Padrões de Validação](#7-padrões-de-validação)
8. [Padrões de API / Endpoints](#8-padrões-de-api--endpoints)
9. [Checklist para Code Review](#9-checklist-para-code-review)

---

## 1. Princípios Clean Code

### Métodos pequenos (20-30 linhas máximo)

Cada método deve ter **uma única responsabilidade** e ser pequeno o suficiente para ser compreendido em segundos. Se um método cresce além de 20-30 linhas, ele provavelmente está fazendo coisas demais e deve ser dividido.

**Exemplo — Método de 3 linhas:**

```csharp
// Category.cs
public void Archive()
{
    IsArchived = true;

    Raise(new CategoryArchivedDomainEvent(Id));
}
```

**Exemplo — Método de 8 linhas:**

```csharp
// Category.cs
public void ChangeName(string name)
{
    if (Name == name)
    {
        return;
    }

    Name = name;

    Raise(new CategoryNameChangedDomainEvent(Id, Name));
}
```

**Exemplo — Método de 12 linhas:**

```csharp
// Event.cs
public Result Publish()
{
    if (Status != EventStatus.Draft)
    {
        return Result.Failure(EventErrors.NotDraft);
    }

    Status = EventStatus.Published;

    Raise(new EventPublishedDomainEvent(Id));

    return Result.Success();
}
```

### Nomes significativos

- **Classes**: representam conceitos claros do domínio (`Event`, `Category`, `CreateEventCommandHandler`)
- **Métodos**: verbos que descrevem a ação (`Publish()`, `Cancel()`, `Archive()`, `ChangeName()`)
- **Variáveis/Parâmetros**: descrevem o conteúdo (`startsAtUtc`, `cancellationToken`, `categoryRepository`)
- **Erros**: identificam claramente o problema (`EventErrors.NotDraft`, `EventErrors.AlreadyCanceled`, `CategoryErrors.NotFound`)

### Uma responsabilidade por método

Cada método deve fazer **uma coisa só**:

- `Archive()` → marca como arquivado e levanta evento
- `Publish()` → valida estado, muda status e levanta evento
- `Cancel()` → valida pré-condições, muda status e levanta evento

---

## 2. Guard Clauses

Guard clauses eliminam a necessidade de `if/else` aninhados. A ideia é **retornar cedo** quando uma pré-condição não é atendida, mantendo o fluxo principal do método no nível mais externo de indentação.

### Anti-pattern: Ifs aninhados

```csharp
// ❌ NÃO FAÇA ISSO
public Result Cancel(DateTime utcNow)
{
    if (Status != EventStatus.Canceled)
    {
        if (StartsAtUtc >= utcNow)
        {
            Status = EventStatus.Canceled;
            Raise(new EventCanceledDomainEvent(Id));
            return Result.Success();
        }
        else
        {
            return Result.Failure(EventErrors.AlreadyStarted);
        }
    }
    else
    {
        return Result.Failure(EventErrors.AlreadyCanceled);
    }
}
```

### Pattern correto: Guard Clauses

```csharp
// ✅ FAÇA ASSIM — Event.cs
public Result Cancel(DateTime utcNow)
{
    if (Status == EventStatus.Canceled)
    {
        return Result.Failure(EventErrors.AlreadyCanceled);
    }

    if (StartsAtUtc < utcNow)
    {
        return Result.Failure(EventErrors.AlreadyStarted);
    }

    Status = EventStatus.Canceled;

    Raise(new EventCanceledDomainEvent(Id));

    return Result.Success();
}
```

**Benefícios:**
- Cada validação é independente e fácil de ler
- O fluxo principal (sucesso) fica no nível mais externo
- Fácil de adicionar novas validações sem aumentar a complexidade

### Guard Clauses em Command Handlers

O mesmo princípio se aplica em handlers:

```csharp
// CreateEventCommandHandler.cs
public async Task<Result<Guid>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
{
    // Guard: data no passado
    if (request.StartsAtUtc < dateTimeProvider.UtcNow)
    {
        return Result.Failure<Guid>(EventErrors.StartDateInPast);
    }

    // Guard: categoria não encontrada
    Category? category = await categoryRepository.GetAsync(request.CategoryId, cancellationToken);

    if (category is null)
    {
        return Result.Failure<Guid>(CategoryErrors.NotFound(request.CategoryId));
    }

    // Guard: erro na criação do domínio
    Result<Event> result = Event.Create(
        category,
        request.Title,
        request.Description,
        request.Location,
        request.StartsAtUtc,
        request.EndsAtUtc);

    if (result.IsFailure)
    {
        return Result.Failure<Guid>(result.Error);
    }

    // Fluxo principal (sucesso)
    eventRepository.Insert(result.Value);

    await unitOfWork.SaveChangesAsync(cancellationToken);

    return result.Value.Id;
}
```

---

## 3. Princípios SOLID

### S — Single Responsibility Principle

Cada classe tem **uma única razão para mudar**.

- `CreateEventCommandHandler` → só lida com criação de eventos
- `CreateEventCommandValidator` → só valida o command de criação
- `CreateEvent` (endpoint) → só mapeia a rota HTTP para o command

```csharp
// Um handler = uma responsabilidade
internal sealed class CreateEventCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ICategoryRepository categoryRepository,
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand, Guid>
{
    // Apenas um método público: Handle
}
```

### O — Open/Closed Principle

Aberto para extensão, fechado para modificação. Novos endpoints são adicionados **sem modificar código existente** — basta implementar `IEndpoint`:

```csharp
// Interface base — nunca precisa ser modificada
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}

// Novo endpoint = nova classe, sem tocar no código existente
internal sealed class CreateEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, ISender sender) =>
        {
            // ...
        });
    }
}
```

O mesmo vale para o `ValidationPipelineBehavior` — novos validators são automaticamente aplicados sem modificar o pipeline.

### L — Liskov Substitution Principle

Qualquer implementação de `ICommandHandler<T>` ou `IQueryHandler<T>` pode ser usada de forma intercambiável pelo MediatR:

```csharp
// Command handler para escrita
internal sealed class CreateEventCommandHandler
    : ICommandHandler<CreateEventCommand, Guid> { }

// Query handler para leitura
internal sealed class GetEventQueryHandler
    : IQueryHandler<GetEventQuery, EventResponse> { }
```

### I — Interface Segregation Principle

Interfaces são **pequenas e focadas**. Cada uma expõe apenas o que seus consumidores realmente precisam:

```csharp
// Apenas 2 métodos — exatamente o que o domínio precisa
public interface IEventRepository
{
    Task<Event?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Event @event);
}

// Apenas 1 método — mínimo necessário
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Apenas 1 propriedade
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
```

### D — Dependency Inversion Principle

Handlers dependem de **abstrações**, nunca de implementações concretas:

```csharp
// Todas as dependências são interfaces (abstrações)
internal sealed class CreateEventCommandHandler(
    IDateTimeProvider dateTimeProvider,        // abstração
    ICategoryRepository categoryRepository,   // abstração
    IEventRepository eventRepository,         // abstração
    IUnitOfWork unitOfWork)                   // abstração
    : ICommandHandler<CreateEventCommand, Guid>
{
    // O handler não sabe (nem precisa saber) se o repositório
    // usa EF Core, Dapper, ou qualquer outra tecnologia
}
```

---

## 4. Result Pattern

Em vez de lançar exceções para fluxos de negócio, use o **Result Pattern** para representar sucesso ou falha de forma explícita.

### Tipos base

```csharp
// Result.cs — representa sucesso ou falha
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);
}

// Error.cs — erro tipado com código, descrição e tipo
public record Error(string Code, string Description, ErrorType Type)
{
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

### Guard clauses retornando Result.Failure

```csharp
// No domínio — Event.cs
public static Result<Event> Create(
    Category category,
    string title,
    string description,
    string location,
    DateTime startsAtUtc,
    DateTime? endsAtUtc)
{
    // Guard clause retorna falha tipada
    if (endsAtUtc.HasValue && endsAtUtc < startsAtUtc)
    {
        return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
    }

    var @event = new Event
    {
        Id = Guid.NewGuid(),
        CategoryId = category.Id,
        Title = title,
        Description = description,
        Location = location,
        StartsAtUtc = startsAtUtc,
        EndsAtUtc = endsAtUtc,
        Status = EventStatus.Draft
    };

    @event.Raise(new EventCreatedDomainEvent(@event.Id));

    return @event; // Conversão implícita para Result<Event>
}
```

### Propagação de erros entre camadas

Os erros fluem do domínio para o handler e do handler para o endpoint, sem exceções:

```csharp
// Handler propaga erro do domínio
Result<Event> result = Event.Create(category, request.Title, ...);

if (result.IsFailure)
{
    return Result.Failure<Guid>(result.Error);
}

// Endpoint converte Result em resposta HTTP
Result<Guid> result = await sender.Send(command);
return result.Match(Results.Ok, ApiResults.Problem);
```

### Quando usar exceções vs Result

| Cenário | Use |
|---|---|
| Regra de negócio violada | `Result.Failure()` |
| Entidade não encontrada | `Result.Failure(Error.NotFound(...))` |
| Validação de input | `FluentValidation` + `ValidationPipelineBehavior` |
| Erro de programação (bug) | Exceção (ex: `ArgumentException`) |
| Falha de infraestrutura | Exceção (ex: `DbException`) |

---

## 5. CQRS Pattern

**Command Query Responsibility Segregation** — separa operações de escrita (commands) de leitura (queries).

### Commands (Escrita via EF Core)

Commands alteram o estado do sistema. São **sealed records** que implementam `ICommand<T>`:

```csharp
// Definição do command — sealed record imutável
public sealed record CreateEventCommand(
    Guid CategoryId,
    string Title,
    string Description,
    string Location,
    DateTime StartsAtUtc,
    DateTime? EndsAtUtc) : ICommand<Guid>;

// Handler — internal sealed class
internal sealed class CreateEventCommandHandler(
    IDateTimeProvider dateTimeProvider,
    ICategoryRepository categoryRepository,
    IEventRepository eventRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateEventCommand, Guid>
{
    public async Task<Result<Guid>> Handle(
        CreateEventCommand request,
        CancellationToken cancellationToken)
    {
        // Usa repositórios (EF Core) para escrita
        eventRepository.Insert(result.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return result.Value.Id;
    }
}
```

### Queries (Leitura via Dapper)

Queries apenas lêem dados. São **sealed records** que implementam `IQuery<T>`:

```csharp
// Definição da query
public sealed record GetEventQuery(Guid EventId) : IQuery<EventResponse>;

// Handler — usa Dapper para SQL direto (leitura otimizada)
internal sealed class GetEventQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetEventQuery, EventResponse>
{
    public async Task<Result<EventResponse>> Handle(
        GetEventQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql =
            $"""
             SELECT
                 e.id AS {nameof(EventResponse.Id)},
                 e.title AS {nameof(EventResponse.Title)},
                 e.description AS {nameof(EventResponse.Description)},
                 e.location AS {nameof(EventResponse.Location)},
                 e.starts_at_utc AS {nameof(EventResponse.StartsAtUtc)},
                 e.ends_at_utc AS {nameof(EventResponse.EndsAtUtc)}
             FROM events.events e
             WHERE e.id = @EventId
             """;

        // Dapper mapeia diretamente para DTO de resposta
        // Sem passar pelo domínio — leitura pura
    }
}
```

### Por que separar Commands e Queries?

| Aspecto | Command (Escrita) | Query (Leitura) |
|---|---|---|
| **ORM** | EF Core (change tracking) | Dapper (SQL puro) |
| **Modelo** | Entidades de domínio | DTOs de resposta |
| **Performance** | Foco em consistência | Foco em velocidade |
| **Validação** | FluentValidation | Parâmetros simples |

---

## 6. Domain-Driven Design

### Construtores privados + Factory Methods estáticos

Entidades nunca são criadas com `new` diretamente. O factory method garante que o objeto é criado em um **estado válido**:

```csharp
public sealed class Event : Entity
{
    // Construtor privado — impede criação externa
    private Event()
    {
    }

    // Setters privados — impede modificação externa
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public EventStatus Status { get; private set; }

    // Factory method — único ponto de criação
    public static Result<Event> Create(
        Category category,
        string title,
        string description,
        string location,
        DateTime startsAtUtc,
        DateTime? endsAtUtc)
    {
        // Validação de invariantes
        if (endsAtUtc.HasValue && endsAtUtc < startsAtUtc)
        {
            return Result.Failure<Event>(EventErrors.EndDatePrecedesStartDate);
        }

        var @event = new Event
        {
            Id = Guid.NewGuid(),
            // ... inicialização completa
            Status = EventStatus.Draft  // Estado inicial controlado
        };

        @event.Raise(new EventCreatedDomainEvent(@event.Id));

        return @event;
    }
}
```

### Domain Events via Raise()

Eventos de domínio são levantados **dentro das entidades** quando algo significativo acontece. Eles são despachados automaticamente após a persistência (via Transactional Outbox):

```csharp
// Ao publicar um evento
public Result Publish()
{
    if (Status != EventStatus.Draft)
    {
        return Result.Failure(EventErrors.NotDraft);
    }

    Status = EventStatus.Published;

    Raise(new EventPublishedDomainEvent(Id));  // Notifica o sistema

    return Result.Success();
}
```

### Encapsulamento de estado

Mudanças de estado só acontecem através de **métodos com semântica de negócio**. Nunca via setters públicos:

```csharp
// ✅ CORRETO — mudança via método de negócio
event.Publish();       // valida regras, muda status, levanta evento
event.Cancel(utcNow);  // valida pré-condições, muda status, levanta evento
category.Archive();    // muda flag, levanta evento

// ❌ ERRADO — nunca exponha setters públicos
event.Status = EventStatus.Published;  // Não compila! Setter é private
```

---

## 7. Padrões de Validação

### FluentValidation nos Commands

Cada command tem um validator correspondente — **internal sealed class** que estende `AbstractValidator<T>`:

```csharp
internal sealed class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(c => c.CategoryId).NotEmpty();
        RuleFor(c => c.Title).NotEmpty();
        RuleFor(c => c.Description).NotEmpty();
        RuleFor(c => c.Location).NotEmpty();
        RuleFor(c => c.StartsAtUtc).NotEmpty();
        RuleFor(c => c.EndsAtUtc)
            .Must((cmd, endsAt) => endsAt > cmd.StartsAtUtc)
            .When(c => c.EndsAtUtc.HasValue);
    }
}
```

### Pipeline Behavior para validação automática

O `ValidationPipelineBehavior` intercepta **todos os commands** automaticamente antes de chegarem ao handler. Nenhum handler precisa chamar validação manualmente:

```csharp
internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseCommand
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        ValidationFailure[] validationFailures = await ValidateAsync(request);

        if (validationFailures.Length == 0)
        {
            return await next(); // Sem erros → segue para o handler
        }

        // Com erros → retorna Result.Failure sem chegar no handler
        // ...
    }
}
```

### Validação em duas camadas

| Camada | Responsabilidade | Mecanismo |
|---|---|---|
| **Application** (Validator) | Input inválido (campos vazios, formatos) | FluentValidation |
| **Domain** (Entity) | Regras de negócio (datas, estados) | Guard clauses + `Result.Failure()` |

---

## 8. Padrões de API / Endpoints

### IEndpoint + Minimal API

Cada endpoint é uma classe que implementa `IEndpoint`. São auto-descobertos via reflection, sem necessidade de registro manual:

```csharp
internal sealed class CreateEvent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("events", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(new CreateEventCommand(
                request.CategoryId,
                request.Title,
                request.Description,
                request.Location,
                request.StartsAtUtc,
                request.EndsAtUtc));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ModifyEvents)
        .WithTags(Tags.Events);
    }

    // Request DTO interno — específico do endpoint
    internal sealed class Request
    {
        public Guid CategoryId { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Location { get; init; }
        public DateTime StartsAtUtc { get; init; }
        public DateTime? EndsAtUtc { get; init; }
    }
}
```

### Result.Match() para respostas HTTP

O método `Match` converte o `Result` em resposta HTTP de forma limpa:

```csharp
// Extensão que mapeia Result para resposta HTTP
public static TOut Match<TIn, TOut>(
    this Result<TIn> result,
    Func<TIn, TOut> onSuccess,    // Ex: Results.Ok
    Func<Result<TIn>, TOut> onFailure)  // Ex: ApiResults.Problem
{
    return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
}
```

`ApiResults.Problem` converte o `ErrorType` em status HTTP (RFC ProblemDetails):

| ErrorType | HTTP Status |
|---|---|
| `Validation` | 400 Bad Request |
| `NotFound` | 404 Not Found |
| `Conflict` | 409 Conflict |
| `Failure` / `Problem` | 500 Internal Server Error |

---

## 9. Checklist para Code Review

### Clean Code
- [ ] Métodos têm no máximo 20-30 linhas?
- [ ] Nomes são claros e descritivos?
- [ ] Cada método tem uma única responsabilidade?
- [ ] Não há código duplicado?

### Guard Clauses
- [ ] Não há `if/else` aninhados (máx 1 nível de indentação)?
- [ ] Pré-condições são verificadas com early return?
- [ ] O fluxo de sucesso está no nível mais externo?

### SOLID
- [ ] Classes têm uma única razão para mudar (SRP)?
- [ ] Novos comportamentos são adicionados via extensão, não modificação (OCP)?
- [ ] Interfaces são pequenas e focadas (ISP)?
- [ ] Dependências são injetadas via abstrações (DIP)?

### Result Pattern
- [ ] Fluxos de negócio usam `Result<T>` em vez de exceções?
- [ ] Erros são propagados corretamente entre camadas?
- [ ] Erros têm código e descrição significativos?

### CQRS
- [ ] Commands usam EF Core para escrita?
- [ ] Queries usam Dapper para leitura?
- [ ] Commands e queries são sealed records?
- [ ] Handlers são internal sealed classes?

### DDD
- [ ] Entidades têm construtores privados + factory methods?
- [ ] Setters são privados?
- [ ] Domain events são levantados para mudanças significativas?
- [ ] Invariantes são validadas dentro da entidade?

### Validação
- [ ] Existe um FluentValidation validator para cada command?
- [ ] Validação de input está no validator, regras de negócio no domínio?

### Endpoints
- [ ] Endpoint implementa `IEndpoint`?
- [ ] Request DTO é internal e específico do endpoint?
- [ ] Resposta usa `result.Match(Results.Ok, ApiResults.Problem)`?
- [ ] Autorização está configurada?
