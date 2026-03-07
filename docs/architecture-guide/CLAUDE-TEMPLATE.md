# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Arquitetura

Este projeto segue o padrao **Modular Monolith com Clean Architecture** conforme documentado em `C:\Repos\Thacio\architecture-guide\`.

Antes de gerar ou modificar codigo, leia os seguintes arquivos de referencia:
- `C:\Repos\Thacio\architecture-guide\01-visao-geral.md` — Estrutura, principios e dependencias entre camadas
- `C:\Repos\Thacio\architecture-guide\02-common-domain.md` — Entity, Result<T>, DomainEvent, Error
- `C:\Repos\Thacio\architecture-guide\03-common-application.md` — CQRS, Pipeline Behaviors, exemplos de Command/Query
- `C:\Repos\Thacio\architecture-guide\04-common-infrastructure.md` — Outbox/Inbox, Auth, Cache, EventBus
- `C:\Repos\Thacio\architecture-guide\05-common-presentation.md` — IEndpoint, ApiResults
- `C:\Repos\Thacio\architecture-guide\06-modulo-template.md` — Como criar um novo modulo
- `C:\Repos\Thacio\architecture-guide\09-checklist-novo-projeto.md` — Checklist e pacotes NuGet

## Regras obrigatorias

- Seguir CQRS: Commands escrevem via EF Core, Queries leem via Dapper com SQL puro
- Metodos de dominio retornam `Result<T>`, nunca lançam exceçoes para erros de negocio
- Entidades usam construtores privados com static factory methods
- Commands e Queries sao `sealed record`
- Handlers sao `internal sealed class`
- Validators sao `internal sealed class` e estendem `AbstractValidator<TCommand>`
- Endpoints implementam `IEndpoint` (Minimal API), nunca controllers
- Modulos nunca referenciam projetos de outros modulos diretamente
- Comunicaçao entre modulos e feita exclusivamente via Integration Events
- Cada modulo tem seu proprio schema no PostgreSQL e seu proprio DbContext
- Domain events sao capturados via Transactional Outbox (InsertOutboxMessagesInterceptor)

## Build e testes

```bash
dotnet build SuaApp.sln
dotnet test SuaApp.sln
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"
docker compose up -d
dotnet run --project src/API/SuaApp.Api
```
