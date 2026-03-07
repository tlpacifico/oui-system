# 09 — Checklist para Iniciar um Novo Projeto

## Fase 1: Scaffold da Solution

- [ ] Criar solution: `dotnet new sln -n SuaApp`
- [ ] Criar `Directory.Build.props` na raiz (copiar de 01-visao-geral.md)
- [ ] Criar `.editorconfig` com regras de estilo C#

## Fase 2: Common (copiar e renomear namespace)

- [ ] Criar `src/Common/SuaApp.Common.Domain/`
  - Entity, IDomainEvent, DomainEvent, Result, Error, ErrorType, ValidationError
- [ ] Criar `src/Common/SuaApp.Common.Application/`
  - ICommand, IQuery, handlers interfaces
  - Pipeline behaviors (Exception, Logging, Validation)
  - ApplicationConfiguration (registro MediatR + FluentValidation)
  - Abstraçoes: IDateTimeProvider, ICacheService, IDbConnectionFactory, IEventBus
  - Integration event interfaces e base classes
- [ ] Criar `src/Common/SuaApp.Common.Infrastructure/`
  - InfrastructureConfiguration
  - Outbox: OutboxMessage, InsertOutboxMessagesInterceptor, DomainEventHandlersFactory
  - Inbox: InboxMessage, IntegrationEventHandlersFactory
  - Auth: JWT + Permission system
  - Cache: CacheService
  - Clock: DateTimeProvider
  - Data: DbConnectionFactory
  - EventBus: MassTransit wrapper
  - Serialization: SerializerSettings (Newtonsoft com TypeNameHandling.All)
- [ ] Criar `src/Common/SuaApp.Common.Presentation/`
  - IEndpoint, EndpointExtensions
  - ApiResults, ResultExtensions

## Fase 3: API Host

- [ ] Criar `src/API/SuaApp.Api/` (ASP.NET Core Web API)
- [ ] Configurar Program.cs (ver template)
- [ ] Criar Extensions: ConfigurationExtensions, SwaggerExtensions, MigrationExtensions
- [ ] Criar Middleware: GlobalExceptionHandler, LogContextTraceLogging
- [ ] Criar `appsettings.json` com connection strings, auth, Serilog

## Fase 4: Primeiro Modulo

Seguir o template em `06-modulo-template.md`:

- [ ] Criar 5 projetos (Domain, Application, Infrastructure, Presentation, IntegrationEvents)
- [ ] Implementar primeiro aggregate root no Domain
- [ ] Criar primeiro command + handler + validator
- [ ] Criar primeiro query + handler (Dapper)
- [ ] Configurar DbContext com Outbox/Inbox
- [ ] Criar ProcessOutboxJob e ProcessInboxJob
- [ ] Criar ModuloModule.cs com registro DI
- [ ] Criar primeiro endpoint (IEndpoint)
- [ ] Criar `modules.{modulo}.json` com config de Outbox/Inbox
- [ ] Registrar modulo no Program.cs

## Fase 5: Docker e Infraestrutura

- [ ] Criar `docker-compose.yml` (PostgreSQL, Redis, Keycloak, Seq, Jaeger)
- [ ] Configurar Keycloak realm e clients
- [ ] Exportar realm para `.files/` (import automatico)
- [ ] Criar Dockerfile para API

## Fase 6: Testes

- [ ] Criar testes unitarios para o dominio
- [ ] Criar testes de arquitetura (dependencias entre camadas)
- [ ] Criar testes de isolamento entre modulos
- [ ] Configurar IntegrationTestWebAppFactory com Testcontainers

## Fase 7: Modulos Adicionais

Para cada novo modulo, repetir Fase 4 e adicionar:
- [ ] Teste de isolamento no architecture test global
- [ ] Integration events para comunicaçao com outros modulos
- [ ] Handlers de integration events nos modulos que precisam reagir

## Pacotes NuGet por projeto

### Common.Domain
```
(nenhum pacote externo)
```

### Common.Application
```
MediatR
FluentValidation
FluentValidation.DependencyInjectionExtensions
```

### Common.Infrastructure
```
Microsoft.EntityFrameworkCore
Npgsql.EntityFrameworkCore.PostgreSQL
EFCore.NamingConventions
Dapper
MassTransit
MassTransit.Redis
Quartz
Quartz.Extensions.Hosting
Scrutor
Serilog.AspNetCore
Serilog.Sinks.Seq
StackExchange.Redis
Microsoft.Extensions.Caching.StackExchangeRedis
Newtonsoft.Json
OpenTelemetry.Extensions.Hosting
OpenTelemetry.Instrumentation.AspNetCore
OpenTelemetry.Instrumentation.Http
OpenTelemetry.Instrumentation.EntityFrameworkCore
OpenTelemetry.Exporter.OpenTelemetryProtocol
Npgsql.OpenTelemetry
Microsoft.AspNetCore.Authentication.JwtBearer
```

### Common.Presentation
```
Microsoft.AspNetCore.App (framework reference)
```

### Module.Domain
```
(referencia: Common.Domain)
```

### Module.Application
```
(referencia: Common.Application, Module.Domain)
```

### Module.Infrastructure
```
Microsoft.EntityFrameworkCore.Tools (para migrations)
(referencia: Common.Infrastructure, Module.Application)
```

### Module.Presentation
```
(referencia: Common.Presentation, Module.Application)
```

### Module.IntegrationEvents
```
(referencia: Common.Application — apenas IIntegrationEvent)
```

### Test Projects
```
xunit
xunit.runner.visualstudio
FluentAssertions
Bogus
coverlet.collector
NetArchTest.Rules (architecture tests)
Testcontainers.PostgreSql (integration tests)
Testcontainers.Redis (integration tests)
Microsoft.AspNetCore.Mvc.Testing (integration tests)
```
