# SHS - System Architecture Document

## 1. Architecture Overview

The SHS ERP follows Modular Monolith **Clean Architecture** with **CQRS (Command Query Responsibility Segregation)** pattern, built on .NET 9 with Minimal APIs and Angular 20 frontend.

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌────────────────────┐   ┌──────────────────────────────┐  │
│  │  Angular 20 SPA    │   │  .NET 9 Minimal API          │  │
│  │  					│◄──┤  Endpoints + OpenAPI         │  │
│  │  Tailwind CSS 4    │   │  JWT Authentication          │  │
│  └────────────────────┘   └──────────────┬───────────────┘  │
├──────────────────────────────────────────┼──────────────────┤
│                 APPLICATION LAYER         │                  │
│  ┌───────────────────────────────────────┼───────────────┐  │
│  │  Commands ──► CommandHandlers          │               │  │
│  │  Queries  ──► QueryHandlers            │               │  │
│  │  Validators (FluentValidation)         │               │  │
│  │  Decorators (Logging, Validation)      │               │  │
│  │  Services (ConsignmentService, etc.)   │               │  │
│  └───────────────────────────────────────┼───────────────┘  │
├──────────────────────────────────────────┼──────────────────┤
│                   DOMAIN LAYER            │                  │
│  ┌───────────────────────────────────────┼───────────────┐  │
│  │  Entities (Consignment, Item, etc.)    │               │  │
│  │  Repository Interfaces                 │               │  │
│  │  Enums (Status, PaymentType)           │               │  │
│  │  Presentation Models                   │               │  │
│  └───────────────────────────────────────┼───────────────┘  │
├──────────────────────────────────────────┼──────────────────┤
│              INFRASTRUCTURE LAYER         │                  │
│  ┌───────────────────────────────────────┼───────────────┐  │
│  │  EF Core 9 (ShsDbContext)              │               │  │
│  │  PostgreSQL (Npgsql)                   │               │  │
│  │  Repository Implementations            │               │  │
│  │  Interceptors (SoftDelete, Audit)      │               │  │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Project Structure

```
src/
├── shs.Api/                          # API Layer (Presentation)
│   ├── Presentation/
│   │   └── Endpoints/
│   │       ├── Consignment/          # Consignment endpoints
│   │       │   ├── ConsignmentEndpoints.cs
│   │       │   ├── SuppliersEndpoints.cs
│   │       │   └── Models/           # Request DTOs
│   │       ├── Store/                # Store/inventory endpoints
│   │       │   ├── StoreEndpoints.cs
│   │       │   └── Models/
│   │       └── Users/                # User endpoints
│   ├── ApiConstants.cs               # Route definitions
│   ├── Program.cs                    # App startup
│   └── angular-client/               # Angular Frontend
│       └── src/app/
│           ├── core/                 # Services, guards, interceptors
│           ├── features/             # Feature modules
│           ├── layout/               # App layout
│           └── pages/                # Standalone pages
│
├── shs.Application/                  # Application Layer
│   ├── Abstractions/
│   │   ├── Behaviors/                # Decorators (Logging, Validation)
│   │   └── Messaging/               # CQRS interfaces
│   ├── Consignment/
│   │   ├── Commands/                 # Write operations
│   │   │   ├── CreateConsignment/
│   │   │   ├── UpdateConsignment/
│   │   │   ├── AddConsignmentItem/
│   │   │   ├── UpdateConsignmentItem/
│   │   │   └── DeleteConsignmentItem/
│   │   ├── Queries/                  # Read operations
│   │   │   ├── GetConsignmentById/
│   │   │   ├── SearchConsignments/
│   │   │   └── GetItems/
│   │   └── ConsignmentService.cs     # Domain service
│   ├── Supplier/
│   │   ├── Commands/
│   │   └── Queries/
│   └── Store/                        # (New - in progress)
│       └── Queries/
│
├── shs.Domain/                       # Domain Layer
│   ├── Entities/
│   │   ├── ConsignmentEntity.cs
│   │   ├── ConsignmentItemEntity.cs
│   │   ├── ConsignmentSupplierEntity.cs
│   │   ├── BrandEntity.cs
│   │   ├── TagEntity.cs
│   │   ├── ConsignmentItemTagEntity.cs
│   │   └── EnumDomainValueEntity.cs
│   ├── Enums/
│   │   ├── ConsignmentStatusType.cs
│   │   └── ConsignmentPaymentType.cs
│   ├── Interfaces/
│   │   └── IConsignmentRepository.cs
│   └── Presentation/                 # Response models
│
├── shs.Infrastructure/               # Infrastructure Layer
│   ├── Database/
│   │   ├── ShsDbContext.cs
│   │   ├── Configurations/           # EF Core entity configs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       └── ConsignmentRepository.cs
│   └── InfrastructureServiceCollection.cs
│
└── tests/
    └── shs.Api.Tests/                # Integration tests
```

---

## 3. Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Runtime** | .NET | 9.0.0 |
| **Language** | C# | 13 |
| **API** | ASP.NET Core Minimal APIs | 9.0 |
| **ORM** | Entity Framework Core | 9.0 |
| **Database** | PostgreSQL | via Npgsql 9.0.4 |
| **Authentication** | Firebase JWT Bearer | - |
| **Validation** | FluentValidation | 11.9.0 |
| **DI Scanner** | Scrutor | 4.2.2 |
| **Logging** | Serilog | 8.0.2 |
| **API Docs** | OpenAPI / Scalar | - |
| **Frontend** | Angular | 20 |
| **CSS** | Tailwind CSS | 4 |
| **TypeScript** | TypeScript | 5.8 |

---

## 4. CQRS Pattern Implementation

### 4.1 Interfaces

```
IQuery<TResponse>           ──► IQueryHandler<TQuery, TResponse>
ICommand                    ──► ICommandHandler<TCommand>
ICommand<TResponse>         ──► ICommandHandler<TCommand, TResponse>
```

### 4.2 Pipeline (Decorator Chain)

```
Request ──► ValidationDecorator ──► LoggingDecorator ──► Handler ──► Response
```

- **ValidationDecorator**: Validates commands using FluentValidation before handler execution
- **LoggingDecorator**: Logs query/command execution timing and results

### 4.3 Handler Registration (Automatic via Scrutor)

All handlers implementing `IQueryHandler<,>` or `ICommandHandler<>` are automatically
registered with scoped lifetime by scanning the Application assembly.

---

## 5. Database Design

### 5.1 EF Core Configuration

- **Interceptors:**
  - `SoftDeleteInterceptor` - Converts DELETE to UPDATE (sets IsDeleted flag)
  - `UpdateCreatedUpdatedPropertiesInterceptor` - Auto-populates audit fields
  - `UpdateExternalIdInterceptor<Guid>` - Auto-generates external IDs

- **Global Query Filters:** Soft-deleted entities are automatically excluded from all queries

### 5.2 Pagination

Custom extension method `AsPageWithTotalAsync<T>()` provides standardized pagination:

```csharp
PageWithTotal<T> {
    int Skip,
    int Take,
    IReadOnlyCollection<T> Items,
    int Total
}
```

Default page size: 20 items.

---

## 6. Authentication & Authorization

- **Provider:** Firebase Authentication
- **Token Type:** JWT Bearer
- **Validation:** Issuer, Audience, Lifetime
- **Enforcement:** All API endpoints require authorization via `.RequireAuthorization()`
- **Frontend:** Firebase SDK for login, `AuthInterceptor` injects token in HTTP headers, `AuthGuard` protects routes

---


## 7. Cross-Cutting Concerns

| Concern | Implementation |
|---------|---------------|
| **Logging** | Serilog (Console + File sinks), LoggingDecorator |
| **Validation** | FluentValidation, ValidationDecorator |
| **Soft Delete** | SoftDeleteInterceptor, IHaveSoftDelete interface |
| **Audit Trail** | EntityWithIdAuditable, CreatedBy/UpdatedBy/On fields |
| **Error Handling** | NotFoundException, ArgumentException, middleware |
| **CORS** | Configured for Angular dev server |
| **API Documentation** | OpenAPI with Scalar UI |

---

## 8. Future Architecture Considerations

### 8.1 For Multi-Store Support
- Tenant-based data isolation (shared DB with store_id filter or separate DBs)
- Centralized admin panel vs. per-store dashboards

### 8.1 For E-commerce Integration
- Event-driven stock sync (publish stock changes as events)
- Webhook receivers for marketplace order notifications

### 8.1 For POS Module
- Offline-capable PWA for cash register resilience
- Receipt printer integration via browser print API or dedicated service
- Barcode scanner input handling (keyboard wedge mode)

### 8.1 For Financial Module
- Batch processing for supplier settlements
- Scheduled jobs for overdue consignment alerts
- Integration with payment gateways for supplier payouts
