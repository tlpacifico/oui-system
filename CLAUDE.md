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

Clean Architecture with 4 layers:

```
shs.Api (Presentation)  →  shs.Application  →  shs.Domain  ←  shs.Infrastructure
```

- **shs.Api** — Minimal API endpoints organized by feature folder (Auth/, Inventory/, Pos/, Financial/, Consignment/, Dashboard/, Reports/, Admin/). Also hosts the Angular SPA under `angular-client/`.
- **shs.Application** — Application layer (placeholder, intended for CQRS handlers/validators).
- **shs.Domain** — Entities, enums, interfaces. No external dependencies. All entities inherit `EntityWithIdAuditable` (Id, ExternalId GUID, CreatedBy/UpdatedBy, CreatedOn/UpdatedOn, soft delete).
- **shs.Infrastructure** — EF Core DbContext (`ShsDbContext`, 23 DbSets), entity configurations in `Database/Configurations/`, migrations in `Database/Migrations/`, services (ItemIdGenerator, Email, RbacSeed).
- **shs.Import** — Console app for importing real business data from spreadsheets.

## Key Patterns

- **Endpoint pattern:** Each feature folder has `*Endpoints.cs` files with static `Map*Endpoints(RouteGroupBuilder)` methods, all wired in `Program.cs`.
- **RBAC:** 28 permissions across 6 areas, 4 roles (Admin, Manager, Cashier, Inventory Clerk). Seeded by `RbacSeedService`. Endpoints use `.RequirePermission("permission.name")` extension.
- **Soft delete:** Entities implement `IHaveSoftDelete`. Global query filters exclude deleted records. `SoftDeleteInterceptor` converts DELETE to UPDATE.
- **Audit fields:** `UpdateCreatedUpdatedPropertiesInterceptor` auto-populates CreatedOn/UpdatedOn/CreatedBy/UpdatedBy.
- **ExternalId:** `UpdateExternalIdInterceptor` auto-generates GUIDs for public-facing IDs (entities use int PK internally, GUID externally).
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
