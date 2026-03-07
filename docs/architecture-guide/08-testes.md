# 08 — Estrategia de Testes

## Tres categorias de testes

### 1. Unit Tests (por modulo)

Testam logica de dominio isoladamente. Sem banco, sem DI, sem HTTP.

**Pacotes**: xUnit, FluentAssertions, Bogus

```csharp
public abstract class BaseTest
{
    protected static readonly Faker Faker = new();

    public static void AssertDomainEventWasPublished<TDomainEvent>(Entity entity)
        where TDomainEvent : IDomainEvent
    {
        Assert.Contains(entity.DomainEvents,
            domainEvent => domainEvent is TDomainEvent);
    }
}
```

**Exemplo**:
```csharp
public class PedidoTests : BaseTest
{
    [Fact]
    public void Criar_ComDescricaoValida_DeveRetornarSucesso()
    {
        // Arrange
        string descricao = Faker.Commerce.ProductName();

        // Act
        Result<Pedido> result = Pedido.Criar(descricao);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Descricao.Should().Be(descricao);
        AssertDomainEventWasPublished<PedidoCriadoDomainEvent>(result.Value);
    }

    [Fact]
    public void Criar_ComDescricaoVazia_DeveRetornarFalha()
    {
        Result<Pedido> result = Pedido.Criar(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PedidoErrors.DescricaoVazia);
    }
}
```

### 2. Architecture Tests (por modulo + solution-wide)

Garantem que as regras de dependencia e convençoes sao respeitadas automaticamente.

**Pacote**: NetArchTest.Rules

#### Por modulo — dependencias entre camadas

```csharp
public class LayerTests : BaseTest
{
    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
    {
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(ApplicationAssembly.GetName().Name)
            .GetResult()
            .IsSuccessful
            .Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .IsSuccessful
            .Should().BeTrue();
    }

    [Fact]
    public void PresentationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        Types.InAssembly(PresentationAssembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureAssembly.GetName().Name)
            .GetResult()
            .IsSuccessful
            .Should().BeTrue();
    }
}
```

#### Por modulo — convençoes de naming/visibility

```csharp
[Fact]
public void CommandHandlers_ShouldBe_InternalAndSealed()
{
    Types.InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .Should()
        .NotBePublic()
        .And()
        .BeSealed()
        .GetResult()
        .IsSuccessful
        .Should().BeTrue();
}

[Fact]
public void Commands_ShouldBe_Sealed()
{
    Types.InAssembly(ApplicationAssembly)
        .That()
        .ImplementInterface(typeof(ICommand<>))
        .Or()
        .ImplementInterface(typeof(ICommand))
        .Should()
        .BeSealed()
        .GetResult()
        .IsSuccessful
        .Should().BeTrue();
}

[Fact]
public void Validators_ShouldBe_InternalAndSealed()
{
    Types.InAssembly(ApplicationAssembly)
        .That()
        .Inherit(typeof(AbstractValidator<>))
        .Should()
        .NotBePublic()
        .And()
        .BeSealed()
        .GetResult()
        .IsSuccessful
        .Should().BeTrue();
}
```

#### Solution-wide — isolamento entre modulos

```csharp
[Fact]
public void PedidosModule_ShouldNotHaveDependencyOn_FinanceiroModule()
{
    string[] financeiroPedidosAssemblies = [
        "SuaApp.Modules.Financeiro.Domain",
        "SuaApp.Modules.Financeiro.Application",
        "SuaApp.Modules.Financeiro.Infrastructure",
        "SuaApp.Modules.Financeiro.Presentation"
    ];

    // Nota: IntegrationEvents SAO permitidos como dependencia

    Types.InAssemblies(PedidosAssemblies)
        .Should()
        .NotHaveDependencyOnAny(financeiroPedidosAssemblies)
        .GetResult()
        .IsSuccessful
        .Should().BeTrue();
}
```

### 3. Integration Tests (com Testcontainers)

Testam o fluxo completo usando containers Docker reais.

**Pacotes**: Testcontainers, xUnit, FluentAssertions, Bogus

```csharp
public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>,
    IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17")
        .WithDatabase("suaapp_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:latest")
        .Build();

    // Keycloak container tambem pode ser adicionado

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Substituir connection strings pelos containers
        });

        // Override environment variables
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Database",
            _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__Cache",
            _redisContainer.GetConnectionString());
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
```

**Base test class**:
```csharp
public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    protected readonly ISender Sender;
    protected readonly Faker Faker = new();

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        IServiceScope scope = factory.Services.CreateScope();
        Sender = scope.ServiceProvider.GetRequiredService<ISender>();
    }
}
```

**Exemplo**:
```csharp
public class CriarPedidoTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CriarPedido_ComDadosValidos_DeveRetornarId()
    {
        // Arrange
        var command = new CriarPedidoCommand(
            Faker.Commerce.ProductName(),
            Faker.Random.Decimal(1, 1000));

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }
}
```

## Comandos

```bash
# Todos os testes
dotnet test SuaApp.sln

# Apenas unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Apenas architecture tests
dotnet test --filter "FullyQualifiedName~ArchitectureTests"

# Apenas integration tests (requer Docker)
dotnet test --filter "FullyQualifiedName~IntegrationTests"

# Teste especifico
dotnet test --filter "FullyQualifiedName~CriarPedidoTests.CriarPedido_ComDadosValidos"
```
