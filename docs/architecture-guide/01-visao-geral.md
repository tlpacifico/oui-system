# 01 — Visao Geral da Arquitetura

## O que e Modular Monolith?

Modular Monolith e um estilo arquitetural que combina a **simplicidade operacional de um monolito** com o **isolamento logico de microserviços**. Toda a aplicaçao roda em um unico processo (deploy unico), mas internamente e dividida em modulos independentes com fronteiras bem definidas.

### Comparaçao com outras abordagens

```
┌─────────────────────┬──────────────────────┬─────────────────────────┐
│  Monolito Tradicional│  Modular Monolith    │  Microserviços          │
├─────────────────────┼──────────────────────┼─────────────────────────┤
│ 1 deploy, 1 bloco   │ 1 deploy, N modulos  │ N deploys, N serviços   │
│ Tudo acoplado        │ Modulos isolados     │ Serviços isolados       │
│ Chamadas diretas     │ Integration Events   │ HTTP/gRPC/Mensageria    │
│ 1 banco, 1 schema    │ 1 banco, N schemas   │ N bancos                │
│ Sem fronteiras       │ Fronteiras por DI    │ Fronteiras por rede     │
│ Facil de começar     │ Facil de começar     │ Complexo desde o inicio │
│ Dificil de escalar   │ Migravel p/ micro    │ Escala independente     │
└─────────────────────┴──────────────────────┴─────────────────────────┘
```

### Por que Modular Monolith?

**Vantagens sobre monolito tradicional:**
- Fronteiras claras entre dominios de negocio (cada modulo com seu proprio Domain, Application, Infrastructure e Presentation)
- Isolamento de dados — cada modulo tem seu proprio schema no banco, impedindo queries acopladas entre dominios
- Modulos se comunicam via Integration Events, nao por chamadas diretas, o que torna o acoplamento explicito
- Testes de arquitetura (NetArchTest) garantem automaticamente que nenhum modulo referencia outro diretamente
- Facilita a evoluçao: se um modulo precisar virar microserviço no futuro, as fronteiras ja estao definidas

**Vantagens sobre microserviços:**
- Um unico deploy — sem orquestraçao de containers, service mesh ou API gateway
- Comunicaçao entre modulos e in-memory (MassTransit `UsingInMemory`), sem latencia de rede
- Uma unica base de codigo, um unico pipeline de CI/CD
- Transaçoes locais mais simples (quando necessario, modulos podem compartilhar a mesma instancia de banco)
- Menor complexidade operacional: sem service discovery, circuit breakers distribuidos ou tracing entre serviços separados

### Caminho de migraçao para microserviços

A arquitetura e desenhada para facilitar a extraçao de modulos para serviços independentes quando necessario:

1. **Integration Events** ja sao o contrato de comunicaçao — basta trocar o transporte do MassTransit de `UsingInMemory` para RabbitMQ, Azure Service Bus ou Kafka
2. **Schemas isolados** no PostgreSQL — cada modulo ja opera em seu proprio schema, facilitando a separaçao em bancos distintos
3. **Sem referencias diretas** — modulos so conhecem os `IntegrationEvents` uns dos outros, nunca o Domain ou Application interno
4. O **Outbox/Inbox pattern** ja garante consistencia eventual, que e o mesmo modelo usado em microserviços

Na pratica, extrair um modulo significa: mover os projetos para um novo repositorio, trocar o transporte do MassTransit e apontar para um banco dedicado. O codigo de negocio nao muda.

## Padrao: Modular Monolith com Clean Architecture

Cada modulo e um "mini-sistema" independente com suas proprias camadas, banco (schema), e regras de negocio. Os modulos vivem no mesmo processo (monolito) mas sao isolados como se fossem microservicos.

```
SuaApp/
├── src/
│   ├── API/
│   │   └── SuaApp.Api/                    # Host ASP.NET Core (Minimal API)
│   │       ├── Extensions/
│   │       ├── Middleware/
│   │       ├── OpenTelemetry/
│   │       ├── Program.cs
│   │       ├── appsettings.json
│   │       └── modules.{modulo}.json       # Config por modulo
│   ├── Common/
│   │   ├── SuaApp.Common.Domain/          # Entity, Result<T>, DomainEvent, Error
│   │   ├── SuaApp.Common.Application/     # ICommand, IQuery, Pipeline Behaviors
│   │   ├── SuaApp.Common.Infrastructure/  # Outbox, Inbox, Auth, Cache, EventBus
│   │   └── SuaApp.Common.Presentation/   # IEndpoint, ApiResults
│   └── Modules/
│       ├── ModuloA/
│       │   ├── SuaApp.Modules.ModuloA.Domain/
│       │   ├── SuaApp.Modules.ModuloA.Application/
│       │   ├── SuaApp.Modules.ModuloA.Infrastructure/
│       │   ├── SuaApp.Modules.ModuloA.Presentation/
│       │   └── SuaApp.Modules.ModuloA.IntegrationEvents/
│       └── ModuloB/
│           └── ... (mesma estrutura)
├── test/
│   ├── SuaApp.ArchitectureTests/          # Isolamento entre modulos
│   └── SuaApp.IntegrationTests/           # Testes com Testcontainers
├── docker-compose.yml
├── Directory.Build.props
└── SuaApp.sln
```

## Principios fundamentais

### 1. Isolamento entre modulos
- Modulos **nunca** referenciam projetos de outros modulos diretamente
- Comunicacao entre modulos e feita **exclusivamente** via Integration Events (MassTransit)
- Cada modulo tem seu proprio schema no PostgreSQL
- Testes de arquitetura garantem esse isolamento automaticamente

### 2. CQRS (Command Query Responsibility Segregation)
- **Commands** (escritas) → EF Core com DbContext
- **Queries** (leituras) → Dapper com SQL puro
- Ambos despachados via MediatR

### 3. Result Pattern (sem exceçoes para logica de negocio)
- Metodos de dominio retornam `Result<T>` em vez de lançar exceçoes
- Exceçoes sao reservadas para erros inesperados/infraestrutura

### 4. Transactional Outbox/Inbox
- Eventos de dominio sao salvos na mesma transaçao do banco
- Jobs do Quartz processam periodicamente
- Garante consistencia eventual entre modulos

## Dependencias entre camadas (por modulo)

```
Presentation → Application → Domain
                    ↑
              Infrastructure
```

- **Domain**: zero dependencias externas (apenas .NET base)
- **Application**: depende de Domain + MediatR + FluentValidation (interfaces apenas)
- **Infrastructure**: depende de Application + Domain (implementa as interfaces)
- **Presentation**: depende de Application (envia commands/queries via MediatR)
- **Presentation NÃO depende de Infrastructure** (registrado via DI no host)

## Directory.Build.props (aplicar em todos os projetos)

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AnalysisLevel>latest</AnalysisLevel>
    <AnalysisMode>All</AnalysisMode>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  </PropertyGroup>

  <ItemGroup Condition="'$(MSBuildProjectExtension)' != '.dcproj'">
    <PackageReference Include="SonarAnalyzer.CSharp" Version="9.24.0.89429">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

## Pacotes NuGet principais

| Pacote | Uso |
|--------|-----|
| MediatR 12.x | CQRS (commands, queries, domain events) |
| FluentValidation 11.x | Validaçao de commands |
| EF Core (Npgsql) 8.x | ORM para escritas |
| Dapper 2.x | SQL puro para leituras |
| MassTransit 8.x | Integration events entre modulos |
| MassTransit.Redis | Persistencia de estado de Sagas |
| Quartz 3.x | Jobs de background (Outbox/Inbox) |
| Scrutor 4.x | Decorator pattern para DI (idempotencia) |
| Serilog | Logging estruturado |
| OpenTelemetry | Tracing distribuido |
| Ulid | Geraçao de IDs ordenados |
| NetArchTest.Rules | Testes de arquitetura |
| Testcontainers | Integration tests com Docker |
| Bogus | Geraçao de dados fake para testes |
