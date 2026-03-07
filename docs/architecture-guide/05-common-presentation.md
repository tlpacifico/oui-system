# 05 — Common.Presentation (Endpoints e API Results)

## IEndpoint (Minimal API Pattern)

Cada endpoint e uma classe que implementa `IEndpoint`. Nao ha controllers — tudo usa Minimal API.

```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
```

### Auto-discovery e registro

```csharp
public static class EndpointExtensions
{
    // Escaneia assemblies e registra todos os IEndpoint no DI
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services, params Assembly[] assemblies)
    {
        ServiceDescriptor[] serviceDescriptors = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);
        return services;
    }

    // Mapeia todos os endpoints registrados
    public static IApplicationBuilder MapEndpoints(
        this WebApplication app,
        RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints =
            app.Services.GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder = routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
            endpoint.MapEndpoint(builder);

        return app;
    }
}
```

## ApiResults (mapeamento Error → HTTP ProblemDetails)

```csharp
public static class ApiResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException();

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));
    }
}
```

Mapeamento de `ErrorType` para HTTP status:

| ErrorType | HTTP Status | RFC |
|-----------|-------------|-----|
| Validation | 400 Bad Request | RFC 7231 §6.5.1 |
| Problem | 400 Bad Request | RFC 7231 §6.5.1 |
| NotFound | 404 Not Found | RFC 7231 §6.5.4 |
| Conflict | 409 Conflict | RFC 7231 §6.5.8 |
| Failure | 500 Internal Server Error | RFC 7231 §6.6.1 |

Quando o erro e `ValidationError`, os erros individuais sao incluidos no campo `extensions.errors` do ProblemDetails.

## ResultExtensions (Match pattern)

```csharp
public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result);
    }
}
```

## Exemplo completo de endpoint

```csharp
internal sealed class ObterPedido : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("pedidos/{id}", async (Guid id, ISender sender) =>
        {
            Result<PedidoResponse> result = await sender.Send(
                new ObterPedidoQuery(id));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.GetPedidos)
        .WithTags(Tags.Pedidos);
    }
}

internal sealed class CriarPedido : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("pedidos", async (Request request, ISender sender) =>
        {
            Result<Guid> result = await sender.Send(
                new CriarPedidoCommand(request.Descricao, request.Valor));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.CriarPedidos)
        .WithTags(Tags.Pedidos);
    }

    internal sealed class Request
    {
        public string Descricao { get; init; }
        public decimal Valor { get; init; }
    }
}
```

### Convençoes

- Endpoints sao `internal sealed`
- Cada endpoint e uma classe separada (um arquivo por endpoint)
- O request body e um `Request` record/class nested dentro do endpoint
- Tags para agrupar no Swagger
- Permissions como constantes (ex: `Permissions.GetPedidos`)
- Usa `result.Match(Results.Ok, ApiResults.Problem)` para mapear resultado
