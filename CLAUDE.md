# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

OUI System is an ERP for a circular fashion store in Portugal ("moda circular"). It manages consignment inventory, point-of-sale, supplier settlements, and store credit. The codebase is in Portuguese context (pt-PT locale, EUR currency, NIF tax IDs).

## Tech Stack

- **Backend:** .NET 9 / C# 13, ASP.NET Core Minimal APIs, EF Core 9 + PostgreSQL (Npgsql)
- **Frontend:** Angular 20 (standalone components, signals), TypeScript 5.9, SCSS, PrimeNG
- **Auth:** Firebase JWT Bearer (Google) on both frontend (@angular/fire) and backend
- **Email:** MailKit (SMTP)

## Common Commands

```bash
# Backend
cd src/shs.Api && dotnet run              # API on https://localhost:5001
dotnet build                               # Build entire solution
dotnet build src/shs.Api                   # Build API project only

# Frontend
cd src/shs.Api/angular-client
npm install                                # Install dependencies
npm start                                  # Dev server on http://localhost:4200
ng build --configuration production        # Production build

# Database (run from src/shs.Api or specify --project)
dotnet ef migrations add <Name> --project src/shs.Infrastructure --startup-project src/shs.Api
dotnet ef database update --project src/shs.Infrastructure --startup-project src/shs.Api
```

## Architecture

Clean Architecture com 4 camadas, seguindo o guia arquitetural em `C:\Repos\Thacio\architecture-guide\`:

```
shs.Api (Presentation)  →  shs.Application  →  shs.Domain  ←  shs.Infrastructure
```

- **shs.Api** — Minimal API endpoints organizados por feature folder (Auth/, Inventory/, Pos/, Financial/, Consignment/, Dashboard/, Reports/, Admin/). Tambem hospeda o Angular SPA em `angular-client/`.
- **shs.Application** — Application layer (placeholder, destinado a CQRS handlers/validators).
- **shs.Domain** — Entities, enums, interfaces. Sem dependencias externas. Todas as entidades herdam `EntityWithIdAuditable` (Id, ExternalId GUID, CreatedBy/UpdatedBy, CreatedOn/UpdatedOn, soft delete).
- **shs.Infrastructure** — EF Core DbContext (`ShsDbContext`, 23 DbSets), entity configurations em `Database/Configurations/`, migrations em `Database/Migrations/`, services (ItemIdGenerator, Email, RbacSeed).
- **shs.Import** — Console app para importar dados reais de planilhas.

## Guia Arquitetural de Referencia

Este projeto deve evoluir seguindo os padroes documentados em `C:\Repos\Thacio\architecture-guide\`. Consultar antes de gerar ou modificar codigo:

| Arquivo | Quando consultar |
|---------|-----------------|
| `01-visao-geral.md` | Principios gerais, dependencias entre camadas |
| `02-common-domain.md` | Ao criar/modificar entidades, usar Result\<T\>, DomainEvent, Error |
| `03-common-application.md` | Ao implementar CQRS (Commands, Queries, Validators, Pipeline Behaviors) |
| `04-common-infrastructure.md` | Outbox/Inbox, Cache, EventBus |
| `05-common-presentation.md` | Ao criar endpoints (IEndpoint pattern, ApiResults) |
| `06-modulo-template.md` | Ao extrair features para modulos independentes |
| `09-checklist-novo-projeto.md` | Referencia de pacotes NuGet por camada |

### Padroes a adotar progressivamente

1. **Result Pattern** — Metodos de dominio devem retornar `Result<T>` em vez de lançar exceçoes para erros de negocio (ver `02-common-domain.md`)
2. **CQRS** — Commands (escritas via EF Core) e Queries (leituras via Dapper) despachados por MediatR. Implementar em `shs.Application` (ver `03-common-application.md`)
3. **Pipeline Behaviors** — ValidationPipelineBehavior + RequestLoggingPipelineBehavior + ExceptionHandlingPipelineBehavior (ver `03-common-application.md`)
4. **IEndpoint pattern** — Migrar de metodos estaticos `Map*Endpoints` para classes que implementam `IEndpoint` com auto-discovery (ver `05-common-presentation.md`)
5. **Modularizaçao** — Quando o projeto crescer, extrair features (Inventory, POS, Financial, etc.) para modulos independentes com Domain/Application/Infrastructure/Presentation proprios (ver `06-modulo-template.md`)

### Convençoes de codigo (do guia)

- Commands e Queries: `sealed record`
- Handlers: `internal sealed class`
- Validators: `internal sealed class` estendendo `AbstractValidator<TCommand>`
- Entidades: construtores privados com static factory methods
- Erros de dominio: constantes tipadas por entidade (ex: `ItemErrors.NaoEncontrado`)

## Key Patterns (atuais)

- **Endpoint pattern:** Cada feature folder tem arquivos `*Endpoints.cs` com metodos estaticos `Map*Endpoints(RouteGroupBuilder)`, registrados em `Program.cs`.
- **RBAC:** 28 permissions em 6 areas, 4 roles (Admin, Manager, Cashier, Inventory Clerk). Seed por `RbacSeedService`. Endpoints usam `.RequirePermission("permission.name")`.
- **Soft delete:** Entidades implementam `IHaveSoftDelete`. Query filters globais excluem registros deletados. `SoftDeleteInterceptor` converte DELETE em UPDATE.
- **Audit fields:** `UpdateCreatedUpdatedPropertiesInterceptor` popula automaticamente CreatedOn/UpdatedOn/CreatedBy/UpdatedBy.
- **ExternalId:** `UpdateExternalIdInterceptor` gera GUIDs para IDs publicos (PK int interno, GUID externo).
- **Item lifecycle:** Received → Evaluated → AwaitingAcceptance → ToSell → Sold → Paid (or Returned/Rejected).

## Angular Client Structure

```
src/app/
├── core/auth/          # auth.service, auth.interceptor (JWT), auth.guard, permission.guard, has-permission.directive
├── core/config/        # app.config.ts, firebase.config.ts (from .example template)
├── core/models/        # TypeScript interfaces/DTOs
└── features/           # Lazy-loaded feature pages: admin/, auth/, dashboard/, inventory/, pos/, finance/, reports/
```

All components are standalone (no NgModules). State managed via Angular Signals. Routes in `app.routes.ts`.

## Firebase Setup (Required)

Frontend requires `firebase.config.ts` created from the example template:
```bash
cd src/shs.Api/angular-client
cp src/app/core/config/firebase.config.example.ts src/app/core/config/firebase.config.ts
```
Then fill in Firebase project credentials. Backend needs `Firebase:ProjectId` in user secrets or appsettings.

## Database

PostgreSQL 15+. Connection configured in `InfrastructureServiceCollection.cs`. User secrets ID: `shs-oui-api-secrets`.

## Documentation

Extensive business/technical docs in `docs/` (Portuguese), numbered 01-13. Key references:
- `02-SYSTEM-ARCHITECTURE.md` — Technical architecture decisions
- `04-BUSINESS-RULES.md` — Business logic constraints
- `08-API-ENDPOINTS.md` — API endpoint specifications
- `10-AUTOMAKER-GUIDE.md` — Development kanban cards
- `11-REAL-CONSIGNMENT-FLOW.md` — Consignment business workflow
- `12-PLANO-DESENVOLVIMENTO.md` — Development plan with implementation status
