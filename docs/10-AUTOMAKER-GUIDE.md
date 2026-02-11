# OUI System - Guia Completo para Automaker

## Versão: 2.0 | Última Atualização: 2026-02-11

---

## 1. Visão Geral

O Automaker é uma IDE autônoma que usa agentes IA (Claude Agent SDK) para implementar features descritas em um Kanban board. Este guia contém **tudo** que você precisa para alimentar o Automaker corretamente.

### O que o Automaker precisa para funcionar bem:
1. **Context Files** (`.automaker/context/`) — regras do projeto que os agentes seguem
2. **Kanban Cards** — tarefas bem descritas com especificação técnica clara
3. **Documentação de referência** — os docs que já temos (arquitetura, business rules, etc.)

---

## 2. Pré-requisitos

```bash
# Node.js 22+
node --version

# Claude Code CLI autenticado
claude --version

# Instalar Automaker
git clone https://github.com/AutoMaker-Org/automaker.git
cd automaker
npm install

# Rodar (Windows)
npm run dev:electron    # Desktop (recomendado)
# OU
npm run dev:web         # Browser http://localhost:3007
```

---

## 3. Context Files (COPIAR para o projeto)

Crie a pasta `.automaker/context/` na raiz do projeto `second-hand-shop` e cole os arquivos abaixo.

### 3.1 Estrutura de pastas

```
second-hand-shop/
├── .automaker/
│   └── context/
│       ├── CLAUDE.md
│       ├── ARCHITECTURE.md
│       ├── CODE_QUALITY.md
│       ├── BUSINESS_RULES.md
│       ├── API_PATTERNS.md
│       └── context-metadata.json
├── docs/                    ← documentação de referência
├── src/
│   ├── shs.Api/
│   ├── shs.Application/
│   ├── shs.Domain/
│   └── shs.Infrastructure/
└── tests/
```

### 3.2 CLAUDE.md
```markdown
# OUI System - Second Hand Shop ERP

## Project Overview
ERP system for second-hand clothing stores (brechos) in Brazil.
Manages the full consignment lifecycle: item intake, inventory, POS sales,
commission calculation, supplier settlement, and business intelligence.

## Tech Stack
- Backend: .NET 9, C# 13, ASP.NET Core Minimal APIs, EF Core 9
- Database: PostgreSQL (Npgsql 9.0.4)
- Frontend: Angular 20, PrimeNG, Tailwind CSS 4, TypeScript 5.8
- Auth: Firebase JWT Bearer Token
- Patterns: Clean Architecture, CQRS, Decorator Pattern (Validation + Logging)
- Validation: FluentValidation 11.9
- DI: Scrutor 4.2 (auto-registration)
- Logging: Serilog 8.0

## Build & Run Commands
- Build backend: dotnet build src/shs.Api/shs.Api.csproj
- Run backend: dotnet run --project src/shs.Api/shs.Api.csproj
- Run tests: dotnet test tests/shs.Api.Tests/
- Add EF migration: dotnet ef migrations add {Name} --project src/shs.Infrastructure --startup-project src/shs.Api
- Apply migration: dotnet ef database update --project src/shs.Infrastructure --startup-project src/shs.Api
- Frontend install: cd src/shs.Api/angular-client && npm install
- Frontend dev: cd src/shs.Api/angular-client && ng serve
- Frontend build: cd src/shs.Api/angular-client && ng build

## Package Manager Rules
- Backend: NuGet (dotnet add package). NEVER use paket.
- Frontend: npm. NEVER use yarn or pnpm.

## Important Rules
- ALL text visible to the user (UI labels, error messages, tooltips) MUST be in Brazilian Portuguese (pt-BR)
- Code (variable names, comments, commits) in English
- Currency is BRL (R$), use pt-BR locale for formatting
- Date format: dd/MM/yyyy
- Phone format: (XX) XXXXX-XXXX
```

### 3.3 ARCHITECTURE.md
```markdown
# Architecture Rules - MUST FOLLOW

## CQRS Pattern
- Commands: shs.Application/{Feature}/Commands/{CommandName}/
  - {CommandName}Command.cs (record implementing ICommand or ICommand<TResponse>)
  - {CommandName}CommandHandler.cs (internal sealed class implementing ICommandHandler)
  - {CommandName}CommandValidator.cs (AbstractValidator<T> from FluentValidation)
- Queries: shs.Application/{Feature}/Queries/{QueryName}/
  - {QueryName}Query.cs (record implementing IQuery<TResponse>)
  - {QueryName}QueryHandler.cs (internal sealed class implementing IQueryHandler)
- Handlers are auto-registered via Scrutor. Do NOT manually register in DI.
- Pipeline: Request -> ValidationDecorator -> LoggingDecorator -> Handler -> Response

## Endpoint Pattern (Minimal APIs)
- Files: shs.Api/Presentation/Endpoints/{Feature}/{Feature}Endpoints.cs
- Use MapGroup() with route prefix from ApiConstants.cs
- ALL endpoints MUST call .RequireAuthorization()
- Request DTOs: shs.Api/Presentation/Endpoints/{Feature}/Models/
- Use ISender from MediatR-like pattern to dispatch commands/queries
- Return Results.Ok(), Results.Created(), Results.NotFound(), etc.

## Entity Pattern
- ALL entities extend EntityWithIdAuditable<long>
- ALL entities implement IHaveSoftDelete (IsDeleted, DeletedBy, DeletedOn)
- Internal ID: long (auto-increment). External ID: Guid (for API exposure)
- Navigation properties use Collection<T> not List<T>
- Configure in ShsDbContext with .HasQueryFilter(x => !x.IsDeleted)

## Repository Pattern
- Interface in shs.Domain/Interfaces/I{Feature}Repository.cs
- Implementation in shs.Infrastructure/Database/Repositories/{Feature}Repository.cs
- Use .AsNoTracking() for ALL read queries
- Use AsPageWithTotalAsync() for paginated results
- Register as scoped in InfrastructureServiceCollection.cs

## Frontend Pattern (Angular 20)
- Feature modules: src/app/features/{feature}/
- Components: {feature}-list/, {feature}-detail/, {feature}-form/
- HTTP services: src/app/core/http-services/{feature}.http-service.ts
- Models: src/app/core/models/api.models.ts
- Routes: {feature}.routes.ts (lazy loaded)
- Use Angular signals for state management
- Use PrimeNG components (p-table, p-dialog, p-toast, p-dropdown, etc.)
- Use Reactive Forms for all forms
- No direct API calls from components - always through http-service
```

### 3.4 CODE_QUALITY.md
```markdown
# Code Quality Standards

## Backend (C# / .NET 9)
- Use record types for Commands, Queries, DTOs, and Responses
- Handlers are internal sealed classes with primary constructors
- Use FluentValidation for ALL command validation
- Constructor injection via primary constructors (not field injection)
- No magic strings - use constants (ApiConstants, etc.)
- Async all the way - NEVER use .Result, .Wait(), or Task.Run()
- Use CancellationToken in all async methods
- Use nullable reference types (string? for nullable strings)
- Decimal for all monetary values (NEVER use double/float for money)

## Frontend (Angular / TypeScript)
- Use Angular signals where appropriate (not BehaviorSubject)
- PrimeNG components for ALL UI elements
- Reactive Forms for form handling
- HttpClient via dedicated http-service classes (one per feature)
- Handle loading states and errors in all API calls
- Use Angular's inject() function (not constructor injection)
- Tailwind CSS for custom styling
- Lazy load feature routes

## Database
- NEVER delete data - always soft delete (IsDeleted flag)
- Audit fields on ALL entities (CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
- Use EF Core migrations - NEVER write manual SQL
- Index foreign keys and frequently queried columns
- Use decimal(18,2) for monetary columns

## Testing
- Integration tests for API endpoints
- Test happy path and main validation scenarios
- Use WebApplicationFactory for API tests
- Test files in tests/shs.Api.Tests/

## Git
- Descriptive commit messages starting with verb: Add, Update, Fix, Refactor
- One feature per commit when possible
```

### 3.5 BUSINESS_RULES.md
```markdown
# Critical Business Rules for Agents

## Consignment
- Item ID format: {SupplierInitial}{YYYYMM}{Sequence:0000} (e.g., MS202602001)
- Sequence is per-supplier, per-month, resets monthly
- Default consignment period: 60 days
- Item status lifecycle: Evaluated -> AwaitingAcceptance -> ToSell -> Sold | Returned
- A sold item MUST trigger commission tracking for settlement

## Commission/Settlement
- Each supplier has TWO commission rates:
  - CommissionPercentageInCash (e.g., 40% - store keeps more)
  - CommissionPercentageInProducts (e.g., 30% - store keeps less, incentivizes store credit)
- Settlement calculation:
  - Cash: SupplierPayment = SalePrice - (SalePrice * CashCommission / 100)
  - Credit: SupplierCredit = SalePrice - (SalePrice * ProductCommission / 100)

## POS
- Sale number format: V{YYYYMMDD}-{DailySequence:000} (e.g., V20260211-015)
- One open register per cashier at a time
- Payment methods: Cash, CreditCard, DebitCard, PIX, StoreCredit
- Split payment: max 2 methods, total must >= sale total
- Discounts: 0-10% cashier, 10-20% manager, 20%+ admin
- Cash register closing: compare counted vs expected, flag discrepancy

## Returns
- Defective: exchange within 7 days
- Non-defective: store credit only, within 30 days
- No cash refunds ever
- Returned consigned item reverts to ToSell status

## Alerts
- 30+ days in stock: yellow alert, suggest 10% reduction
- 45+ days: orange alert, suggest 20% reduction
- 60+ days: red alert, suggest return or 30%+ reduction
```

### 3.6 API_PATTERNS.md
```markdown
# API Response Patterns

## Pagination
All list endpoints use standard pagination:
- Query params: ?page=1&pageSize=20
- Response: { data: T[], totalCount: number, page: number, pageSize: number }

## Error Response
{
  "type": "ValidationError" | "NotFound" | "BusinessRuleViolation",
  "title": "Description",
  "status": 400 | 404 | 422,
  "errors": { "FieldName": ["Error message in pt-BR"] }
}

## Existing Endpoints (DO NOT recreate):
- POST /api/consignments - Create consignment
- GET /api/consignments/{id} - Get consignment by ID
- PUT /api/consignments/{id} - Update consignment
- POST /api/consignments/{id}/items - Add item to consignment
- PUT /api/consignments/{id}/items/{itemId} - Update item
- DELETE /api/consignments/{id}/items/{itemId} - Delete item
- GET /api/consignments/search - Search consignments
- GET /api/items - Search items
- POST /api/suppliers - Create supplier
- GET /api/suppliers/{id} - Get supplier
- PUT /api/suppliers/{id} - Update supplier
- GET /api/suppliers/search - Search suppliers
- GET /api/brands - List brands
- POST /api/brands - Create brand
- GET /api/tags - List tags
- POST /api/tags - Create tag

## New Endpoints to Create:
See docs/08-API-ENDPOINTS.md for the complete list of ~135 endpoints needed.
```

### 3.7 context-metadata.json
```json
{
  "files": {
    "CLAUDE.md": {
      "description": "Project overview, tech stack, build commands, package manager rules, and localization requirements"
    },
    "ARCHITECTURE.md": {
      "description": "CQRS patterns, endpoint structure, entity patterns, repository patterns, and frontend patterns to follow strictly"
    },
    "CODE_QUALITY.md": {
      "description": "Code quality standards for backend C#, frontend Angular, database, testing, and git conventions"
    },
    "BUSINESS_RULES.md": {
      "description": "Critical business rules for consignment, commission, POS, returns, and inventory alerts"
    },
    "API_PATTERNS.md": {
      "description": "API response patterns, pagination, error format, existing endpoints, and new endpoints to create"
    }
  }
}
```

---

## 4. Kanban Cards — Completos por Fase

### FASE 1: POS (MVP) — 6 Cards

#### Card 1.1: POS - Entidades do Banco de Dados
```
Title: Create POS database entities and migrations

Priority: P0
Model: Claude Opus
Ultrathink: OFF

Specification:
Create EF Core entities following existing patterns in shs.Domain/Entities/.

ENTITIES TO CREATE:

1. CashRegisterEntity (extends EntityWithIdAuditable<long>, IHaveSoftDelete)
   - OperatorUserId (string, required) - Firebase UID
   - OperatorName (string, required) - display name
   - RegisterNumber (int) - cash register station number
   - OpenedAt (DateTime, required)
   - ClosedAt (DateTime?)
   - OpeningAmount (decimal, required) - initial float
   - ClosingAmount (decimal?) - counted at closing
   - ExpectedAmount (decimal?) - calculated
   - Discrepancy (decimal?) - ClosingAmount - ExpectedAmount
   - DiscrepancyNotes (string?)
   - Status (CashRegisterStatus enum: Open=1, Closed=2)
   - Navigation: Sales (Collection<SaleEntity>)

2. SaleEntity (extends EntityWithIdAuditable<long>, IHaveSoftDelete)
   - SaleNumber (string, required, unique) - format: V{YYYYMMDD}-{Seq:000}
   - CashRegisterId (long, FK -> CashRegister)
   - CustomerId (long?, FK -> nullable, for future loyalty)
   - SaleDate (DateTime, required)
   - Subtotal (decimal) - sum of item prices before discount
   - DiscountPercentage (decimal, default 0)
   - DiscountAmount (decimal, default 0)
   - TotalAmount (decimal) - final amount after discount
   - DiscountReason (string?)
   - Status (SaleStatus: Active=1, Voided=2, PartialReturn=3, FullReturn=4)
   - Notes (string?)
   - Navigation: Items (Collection<SaleItemEntity>)
   - Navigation: Payments (Collection<SalePaymentEntity>)
   - Navigation: CashRegister (CashRegisterEntity)

3. SaleItemEntity (no soft delete needed)
   - Id (long, PK, auto-increment)
   - SaleId (long, FK -> Sale)
   - ConsignmentItemId (long, FK -> ConsignmentItem)
   - UnitPrice (decimal) - price at time of sale
   - DiscountAmount (decimal, default 0)
   - FinalPrice (decimal) - UnitPrice - DiscountAmount
   - Navigation: Sale, ConsignmentItem

4. SalePaymentEntity (no soft delete needed)
   - Id (long, PK, auto-increment)
   - SaleId (long, FK -> Sale)
   - PaymentMethod (PaymentMethodType: Cash=1, CreditCard=2, DebitCard=3, PIX=4, StoreCredit=5)
   - Amount (decimal, required)
   - Reference (string?) - card auth code, PIX ID, etc.
   - Navigation: Sale

5. ENUMS to create in shs.Domain/Enums/:
   - CashRegisterStatus (Open=1, Closed=2)
   - SaleStatus (Active=1, Voided=2, PartialReturn=3, FullReturn=4)
   - PaymentMethodType (Cash=1, CreditCard=2, DebitCard=3, PIX=4, StoreCredit=5)

6. Add DbSets to ShsDbContext.cs
7. Create EF Configurations for each entity
8. Create migration: dotnet ef migrations add AddPosEntities

REFERENCE FILES:
- shs.Domain/Entities/ConsignmentEntity.cs (entity pattern)
- shs.Infrastructure/Database/ShsDbContext.cs (context pattern)
- shs.Infrastructure/Database/Configurations/ (config pattern)
```

#### Card 1.2: POS - Cash Register Backend
```
Title: Implement Cash Register open/close/status endpoints

Priority: P0
Model: Claude Opus
Ultrathink: OFF

Specification:
Create CQRS handlers and endpoints for cash register management.

COMMANDS:
1. OpenCashRegisterCommand(decimal OpeningAmount)
   - Validate: no other open register for current user
   - Create CashRegisterEntity with Status=Open, OpenedAt=DateTime.UtcNow
   - Get OperatorUserId and OperatorName from auth context
   - Return: { registerId, registerNumber, openedAt }
   - Validation: OpeningAmount >= 0

2. CloseCashRegisterCommand(Guid RegisterExternalId, decimal ClosingAmount, string? Notes)
   - Validate: register exists, belongs to current user, is Open
   - Calculate ExpectedAmount = OpeningAmount + CashSalesTotal - CashReturnsTotal
   - Set Discrepancy = ClosingAmount - ExpectedAmount
   - Set Status=Closed, ClosedAt=DateTime.UtcNow
   - Return: { salesCount, totalByMethod, expectedCash, countedCash, discrepancy }

QUERIES:
1. GetCurrentCashRegisterQuery() - returns current user's open register or null
2. GetCashRegisterByIdQuery(Guid ExternalId) - returns register details + sales summary
3. GetAllCashRegistersStatusQuery() - returns all registers (for manager monitoring)

REPOSITORY:
- Create IPosRepository in shs.Domain/Interfaces/
- Create PosRepository in shs.Infrastructure/Database/Repositories/

ENDPOINTS (in PosEndpoints.cs):
- POST /api/pos/register/open -> OpenCashRegisterCommand
- POST /api/pos/register/close -> CloseCashRegisterCommand
- GET /api/pos/register/current -> GetCurrentCashRegisterQuery
- GET /api/pos/register/{id} -> GetCashRegisterByIdQuery
- GET /api/pos/register/status -> GetAllCashRegistersStatusQuery

REFERENCE: ConsignmentEndpoints.cs, ConsignmentRepository.cs
```

#### Card 1.3: POS - Process Sale Backend
```
Title: Implement sale processing endpoint with commission tracking

Priority: P0
Model: Claude Opus
Ultrathink: ON

Specification:
This is the most critical endpoint. A sale must:
1. Record the sale
2. Update inventory (item status -> Sold)
3. Track commission for future settlement

COMMAND: ProcessSaleCommand
Input:
{
  cashRegisterId: Guid,
  items: [{ consignmentItemExternalId: Guid, discountAmount: decimal }],
  payments: [{ method: PaymentMethodType, amount: decimal }],
  discountPercentage: decimal?,
  discountReason: string?,
  customerExternalId: Guid?,
  notes: string?
}

Business Logic:
1. Validate cash register is Open and belongs to current user
2. For each item:
   a. Validate ConsignmentItem exists
   b. Validate status is ToSell (not already Sold/Returned)
   c. Get EvaluatedValue as UnitPrice
   d. Calculate FinalPrice = UnitPrice - itemDiscountAmount
3. Calculate Subtotal = sum of all UnitPrices
4. Calculate TotalDiscount = (Subtotal * discountPercentage/100) + sum of item discounts
5. Calculate TotalAmount = Subtotal - TotalDiscount
6. Validate sum of payment amounts >= TotalAmount
7. Generate SaleNumber: V{YYYYMMDD}-{DailySequence:000}
   - Query today's max sequence, increment by 1
8. Create SaleEntity with SaleItems and SalePayments
9. For each SaleItem: update ConsignmentItem.Status = Sold
10. Return: { saleId, saleNumber, totalAmount, change (if cash > total) }

VALIDATIONS (FluentValidation):
- At least 1 item required
- At least 1 payment required
- All item IDs must exist and be ToSell
- Payment total must >= sale total
- DiscountPercentage 0-100
- If discountPercentage > 10, log warning (should require approval)

QUERY: GetSaleByIdQuery(Guid ExternalId)
- Returns: sale with items (item name, brand, supplier), payments, cashier name

QUERY: GetSalesTodayQuery()
- Returns: { salesCount, totalRevenue, averageTicket, byPaymentMethod }

ENDPOINTS:
- POST /api/pos/sales -> ProcessSaleCommand
- GET /api/pos/sales/{id} -> GetSaleByIdQuery
- GET /api/pos/sales/today -> GetSalesTodayQuery
- GET /api/pos/sales?dateFrom=&dateTo=&page=&pageSize= -> SearchSalesQuery

REFERENCE: docs/04-BUSINESS-RULES.md (RN-POS-01 to RN-POS-06)
```

#### Card 1.4: POS - Frontend (Tela de Venda)
```
Title: Create POS sale screen in Angular

Priority: P0
Model: Claude Sonnet
Ultrathink: OFF

Specification:
Create Angular feature module: src/app/features/pos/

COMPONENTS:

1. pos-register/ (open/close cash register)
   - If no register open: show "Abrir Caixa" form with opening amount field
   - If register open: show register info (operator, opened at, register number)
   - "Fechar Caixa" button -> dialog with closing amount field

2. pos-sale/ (main sale screen - FULL SCREEN layout, hide sidebar)
   Layout as two columns:
   LEFT (60%):
   - Search input at top (autofocus, placeholder: "Escanear código ou buscar item...")
   - API call on enter/debounce: GET /api/items?search=X&status=ToSell
   - Results as cards showing: item name, brand, size, price, supplier, "Adicionar" button
   RIGHT (40%):
   - Cart header: "Carrinho"
   - List of added items with: name, brand, price, remove button
   - Discount section: percentage input + calculated amount
   - Subtotal, Discount, TOTAL (large font)
   - "Finalizar Venda" button (opens payment dialog)
   Bottom bar:
   - Customer info (optional)
   - Keyboard shortcuts: F2=Nova, F4=Pagar, F8=Busca, ESC=Cancelar

3. pos-payment-dialog/ (modal for payment)
   - Payment method dropdown (Dinheiro, Cartão Crédito, Cartão Débito, PIX, Crédito Loja)
   - Amount field (pre-filled with total)
   - "Dividir pagamento" toggle -> shows second payment method
   - For cash: show "Valor recebido" field and calculate "Troco"
   - "Confirmar Pagamento" button -> POST /api/pos/sales
   - After success: show receipt summary with print option

4. pos-sales-list/ (today's sales list)
   - Table with: Nº Venda, Hora, Qtd Itens, Total, Forma Pgto, Ações (ver, imprimir)

HTTP SERVICE: src/app/core/http-services/pos.http-service.ts
- openRegister(openingAmount): POST /api/pos/register/open
- closeRegister(id, closingAmount, notes): POST /api/pos/register/close
- getCurrentRegister(): GET /api/pos/register/current
- searchItems(query): GET /api/items?search=X&status=ToSell
- processSale(saleData): POST /api/pos/sales
- getSale(id): GET /api/pos/sales/{id}
- getTodaySales(): GET /api/pos/sales/today

ROUTES: pos.routes.ts
- /pos -> pos-register (check if register is open)
- /pos/sale -> pos-sale (requires open register)
- /pos/sales -> pos-sales-list

Add menu item in app.menu.ts: "Ponto de Venda" with icon pi-shopping-cart

ALL LABELS IN BRAZILIAN PORTUGUESE.
Use PrimeNG components. Tailwind for custom styling.
```

#### Card 1.5: User Roles & Permissions
```
Title: Implement role-based access control (RBAC)

Priority: P0
Model: Claude Sonnet
Ultrathink: OFF

Specification:
Currently all authenticated users have the same access.
Implement role-based permissions.

BACKEND:
1. Create UserRoleEntity or use Firebase custom claims
   - Roles: Admin, Manager, Finance, Cashier
2. Create middleware/filter to check roles on endpoints
3. Add role requirement to each endpoint group:
   - POS endpoints: Cashier+
   - Inventory write: Manager+
   - Inventory read: Cashier+
   - Finance/Settlement: Finance+
   - Admin/Users: Admin only
   - Reports: Manager+

FRONTEND:
1. Store user role in auth service after login
2. Create RoleGuard for route protection
3. Show/hide menu items based on role
4. Show/hide action buttons based on role

REFERENCE: docs/04-BUSINESS-RULES.md section 8 (RN-USR-01, RN-USR-02)
See the permissions table for complete role x permission mapping.
```

#### Card 1.6: Barcode & Label Printing
```
Title: Implement barcode generation and label printing

Priority: P0
Model: Claude Sonnet
Ultrathink: OFF

Specification:
Each item needs a printable label with barcode.

BACKEND:
1. Generate barcode image from item IdentificationNumber
   - Use a .NET barcode library (e.g., ZXing.Net or BarcodeLib)
   - Generate Code128 barcode as PNG/SVG
2. Endpoint: GET /api/items/{id}/barcode -> returns barcode image
3. Endpoint: POST /api/items/batch/labels -> body: { itemIds: Guid[] }
   - Returns HTML template with labels for printing

LABEL LAYOUT (each label ~5cm x 3cm):
- Barcode (Code128)
- Item ID text below barcode
- Item name (truncated)
- Brand
- Size
- Price (R$ XX,XX in large font)

FRONTEND:
1. "Imprimir Etiqueta" button on item detail page
2. "Imprimir Etiquetas" batch button on items list (select multiple)
3. Opens print preview in new window using window.print()
```

---

### FASE 2: Financeiro — 4 Cards

#### Card 2.1: Settlement - Entidades
```
Title: Create settlement database entities and migrations

Priority: P1
Model: Claude Sonnet

Specification:
Create entities for consignment settlement (acerto com fornecedor).

ENTITIES:

1. SettlementEntity (extends EntityWithIdAuditable<long>, IHaveSoftDelete)
   - SettlementNumber (string, unique) - format: AC{YYYYMM}-{Seq:000}
   - SupplierId (long, FK -> ConsignmentSupplier)
   - PeriodStart (DateTime)
   - PeriodEnd (DateTime)
   - TotalSalesAmount (decimal) - sum of all item sale prices
   - TotalCommission (decimal) - store's commission total
   - TotalPayable (decimal) - amount to pay supplier
   - PaymentType (SettlementPaymentType: Cash=1, StoreCredit=2)
   - CommissionRate (decimal) - rate used (cash or credit)
   - Status (SettlementStatus: Pending=1, Paid=2, Cancelled=3)
   - PaidAt (DateTime?)
   - Notes (string?)
   - Navigation: Items (Collection<SettlementItemEntity>)
   - Navigation: Supplier (ConsignmentSupplierEntity)

2. SettlementItemEntity
   - Id (long, PK)
   - SettlementId (long, FK -> Settlement)
   - ConsignmentItemId (long, FK -> ConsignmentItem)
   - SaleId (long, FK -> Sale)
   - SalePrice (decimal)
   - CommissionRate (decimal)
   - CommissionAmount (decimal)
   - SupplierAmount (decimal)

3. ENUMS:
   - SettlementPaymentType (Cash=1, StoreCredit=2)
   - SettlementStatus (Pending=1, Paid=2, Cancelled=3)

4. Create migration: dotnet ef migrations add AddSettlementEntities
```

#### Card 2.2: Settlement - Backend
```
Title: Implement settlement calculation and processing endpoints

Priority: P1
Model: Claude Opus
Ultrathink: ON

Specification:
See docs/04-BUSINESS-RULES.md (RN-SET-01 to RN-SET-04).

QUERIES:
1. GetPendingSettlementsQuery(Guid? SupplierExternalId)
   - Find all ConsignmentItems where Status=Sold AND not yet in a Settlement
   - Group by Supplier
   - For each supplier calculate:
     - Items sold count
     - Total sales amount
     - Commission if Cash (using CommissionPercentageInCash)
     - Payable if Cash
     - Commission if Credit (using CommissionPercentageInProducts)
     - Payable if Credit
   - Return list of pending settlements per supplier

2. PreviewSettlementQuery(Guid SupplierExternalId, DateTime PeriodStart, DateTime PeriodEnd, SettlementPaymentType PaymentType)
   - Returns detailed preview with per-item breakdown
   - Does NOT create anything, just calculates

3. GetSettlementByIdQuery(Guid ExternalId)
   - Returns settlement with items, supplier info

4. SearchSettlementsQuery(filters, pagination)
   - Filters: supplierId, status, dateRange

COMMANDS:
1. CreateSettlementCommand(Guid SupplierExternalId, DateTime PeriodStart, DateTime PeriodEnd, SettlementPaymentType PaymentType, string? Notes)
   - Get all sold items for supplier in period that have no settlement
   - Apply correct commission rate (Cash vs Credit)
   - Create SettlementEntity + SettlementItems
   - Generate SettlementNumber: AC{YYYYMM}-{Seq:000}
   - Return settlement summary

ENDPOINTS:
- GET /api/finance/settlements/pending -> GetPendingSettlementsQuery
- POST /api/finance/settlements/preview -> PreviewSettlementQuery
- POST /api/finance/settlements -> CreateSettlementCommand
- GET /api/finance/settlements/{id} -> GetSettlementByIdQuery
- GET /api/finance/settlements -> SearchSettlementsQuery
```

#### Card 2.3: Settlement - Frontend
```
Title: Create settlement management Angular pages

Priority: P1
Model: Claude Sonnet

Specification:
Feature module: src/app/features/finance/

PAGES:
1. settlements-list/ - List with tabs: Pendentes | Processados | Todos
   - Pendentes tab: table with Fornecedor, Período, Itens, Total Vendas, Comissão, A Pagar, [Processar]
   - Processados tab: table with Nº Acerto, Fornecedor, Data, Total, Status

2. settlement-new/ - Process new settlement
   - Step 1: Select supplier + period (date range)
   - Step 2: Choose payment type (Dinheiro or Crédito em Loja)
     - Show recalculated values when switching
   - Step 3: Review item-by-item breakdown table
   - Step 4: Confirm -> create settlement
   - Show receipt/summary after

3. settlement-detail/ - View processed settlement
   - Header: Nº, Fornecedor, Período, Status
   - Items table: Item ID, Nome, Data Venda, Preço, Comissão%, Comissão R$, Valor Fornecedor
   - Totals: Total vendido, Comissão total, Total pago
   - Print button

Routes: /finance/settlements, /finance/settlements/new, /finance/settlements/:id
Menu: "Financeiro" > "Acertos" with icon pi-wallet
ALL LABELS IN BRAZILIAN PORTUGUESE.
```

#### Card 2.4: Store Credits
```
Title: Implement store credits management

Priority: P1
Model: Claude Sonnet

Specification:
Store credits are generated by: returns, supplier payments in credit.
Can be used as payment method in POS.

ENTITY: StoreCreditEntity
- HolderType (CreditHolderType: Customer=1, Supplier=2)
- HolderName (string)
- HolderExternalId (Guid) - customer or supplier external ID
- OriginalAmount (decimal)
- CurrentBalance (decimal)
- IssuedAt (DateTime)
- ExpiresAt (DateTime) - 180 days after issuance
- Status (CreditStatus: Active=1, Exhausted=2, Expired=3)

ENTITY: StoreCreditTransactionEntity
- StoreCreditId (long, FK)
- Type (CreditTransactionType: Issue=1, Redemption=2, Expiration=3)
- Amount (decimal)
- ReferenceType (string?) - "Sale", "Settlement", "Return"
- ReferenceId (Guid?)
- Notes (string?)

ENDPOINTS:
- GET /api/finance/credits -> list credits with filters
- GET /api/finance/credits/{id} -> detail with transactions
- POST /api/finance/credits -> manual credit creation
- GET /api/finance/credits/search?holder=X -> for POS search

FRONTEND: credits-list, credit-detail pages under finance module
```

---

### FASE 3: Reports & Dashboard — 3 Cards

#### Card 3.1: Dashboard API
```
Title: Create executive dashboard API endpoint

Priority: P1
Model: Claude Sonnet

Specification:
Read-only query that aggregates KPIs.

QUERY: GetDashboardQuery(string Period = "today")
Returns:
{
  salesToday: { count, revenue, averageTicket },
  salesMonth: { count, revenue, averageTicket, growthPercent },
  inventory: { totalItems, totalValue, stagnantCount },
  pendingSettlements: { totalAmount, suppliersCount },
  topSellingItems: [{ name, brand, price, soldDate }] (top 5 this week),
  alerts: {
    expiringConsignments: count,
    stagnantItems30: count,
    stagnantItems45: count,
    stagnantItems60: count,
    openRegisters: [{ operatorName, openedAt, salesCount }]
  },
  salesChart: [{ date, revenue, count }] (last 7 or 30 days)
}

ENDPOINT: GET /api/dashboard?period=today|week|month
```

#### Card 3.2: Reports API
```
Title: Create reports endpoints (sales, inventory, suppliers, finance)

Priority: P1
Model: Claude Sonnet

Specification:
4 report endpoints, all read-only:

1. GET /api/reports/sales?startDate=&endDate=&brandId=&categoryId=
   Returns: revenue, salesCount, avgTicket, topBrands, topCategories,
   paymentBreakdown, dailySalesChart, previousPeriodComparison

2. GET /api/reports/inventory
   Returns: totalItems, totalValue, agingDistribution (0-15, 15-30, 30-45, 45-60, 60+),
   sellThroughRate by category/brand, stagnantItemsList

3. GET /api/reports/suppliers?startDate=&endDate=
   Returns: ranking by revenue, avgDaysToSell, sellThroughRate,
   returnRate, pendingAmount per supplier

4. GET /api/reports/finance?startDate=&endDate=
   Returns: grossRevenue, commissionRevenue, pendingSettlements,
   paidSettlements, projectedCashflow

All reports support: ?format=json (default) | pdf | excel
For PDF/Excel: generate file and return download URL
```

#### Card 3.3: Dashboard & Reports Frontend
```
Title: Create Dashboard and Reports Angular pages

Priority: P1
Model: Claude Sonnet

Specification:
1. Dashboard page (src/app/features/dashboard/)
   - 4 KPI cards at top (vendas hoje, receita mês, itens estoque, acertos pendentes)
   - Sales chart (last 7 days bar chart) using PrimeNG p-chart
   - Alerts panel (consignações expirando, estoque parado, caixas abertos)
   - Top 5 sales table
   - Quick action buttons (Nova Venda, Nova Consignação, Buscar Item)

2. Reports pages (src/app/features/reports/)
   - reports-sales/ - KPIs, charts (bar + pie), top brands table, export buttons
   - reports-inventory/ - aging distribution, sell-through rates
   - reports-suppliers/ - ranking table, performance metrics
   - reports-finance/ - revenue breakdown, projections

Use PrimeNG p-chart for all charts.
ALL LABELS IN BRAZILIAN PORTUGUESE.
```

---

### FASE 4: Funcionalidades Extras — 5 Cards

#### Card 4.1: Supplier Portal
```
Title: Create supplier self-service portal (read-only)

Model: Claude Sonnet
Spec: Separate Angular module at /portal routes. Login with supplier
email/password (Firebase). Dashboard with: items in store, items sold,
amount receivable, total received. Items list with status filter.
Statements list with receipt PDF download. ALL read-only.
Backend: /api/portal/* endpoints with supplier auth middleware.
```

#### Card 4.2: Promotions Engine
```
Title: Implement promotions/campaigns management

Model: Claude Sonnet
Spec: PromotionEntity with: name, startDate, endDate, type (PercentByCategory,
PercentByAge, Progressive, BuyXGetY), rules (JSON), status. Endpoints to
CRUD promotions. GET /api/promotions/active for POS to auto-apply discounts.
Frontend: promotions list, create/edit form with dynamic rules based on type.
```

#### Card 4.3: Customer & Loyalty
```
Title: Implement customer management and loyalty program

Model: Claude Sonnet
Spec: CustomerEntity with: name, cpf, email, phone, birthDate.
LoyaltyPointsEntity with: customerId, points, type (Earned/Redeemed),
referenceType, referenceId. Config: pointsPerReal, redemptionRate,
expirationMonths. Integration with POS: identify customer, earn points,
redeem points as payment discount. Birthday month 10% discount.
```

#### Card 4.4: Inventory Alerts & Notifications
```
Title: Implement inventory alerts and in-app notifications

Model: Claude Sonnet
Spec: NotificationEntity: userId, title, message, type, isRead, referenceType,
referenceId. Background job (Hangfire or hosted service) runs daily:
check items 30+/45+/60+ days, check expiring consignments, generate
notifications. API: GET /api/notifications, PATCH /api/notifications/{id}/read.
Frontend: bell icon in header with badge count, dropdown with notification list.
Alerts page: /inventory/alerts with color-coded table and quick action buttons.
```

#### Card 4.5: Audit Log
```
Title: Implement comprehensive audit logging

Model: Claude Sonnet
Spec: AuditLogEntity: timestamp, userId, userName, action (Create/Update/Delete/
PriceChange/Discount/VoidSale/Login), module, entityType, entityId, oldValues
(JSON), newValues (JSON), ipAddress. Use EF Core interceptor to auto-log
changes. API: GET /api/admin/audit-log with filters. Frontend: table with
expandable detail rows, export CSV. Admin access only.
```

---

## 5. Ordem de Execução Recomendada

```
FASE 1 (Semanas 1-3):
Card 1.1 (POS Entities)        -> PRIMEIRO (fundação do banco)
Card 1.2 (Cash Register)       -> Após 1.1
Card 1.3 (Process Sale)        -> Após 1.2 ⭐ MAIS CRÍTICO
Card 1.5 (RBAC)                -> Paralelo com 1.2
Card 1.4 (POS Frontend)        -> Após 1.2 + 1.3
Card 1.6 (Barcode)             -> Paralelo com 1.4

FASE 2 (Semanas 4-5):
Card 2.1 (Settlement Entities) -> Após 1.3
Card 2.2 (Settlement Backend)  -> Após 2.1 ⭐ COMPLEXO
Card 2.3 (Settlement Frontend) -> Após 2.2
Card 2.4 (Store Credits)       -> Paralelo com 2.3

FASE 3 (Semanas 6-7):
Card 3.1 (Dashboard API)       -> Após Fase 2
Card 3.2 (Reports API)         -> Paralelo com 3.1
Card 3.3 (Dashboard Frontend)  -> Após 3.1 + 3.2

FASE 4 (Semanas 8-10):
Cards 4.1-4.5 podem rodar em paralelo (agentes concorrentes)
```

---

## 6. Dicas de Uso do Automaker

| Dica | Detalhe |
|------|---------|
| **Modelo para cards complexos** | Use Claude Opus para cards com lógica de negócio (POS, Settlement) |
| **Modelo para cards simples** | Use Claude Sonnet para CRUD e frontend |
| **Ultrathink** | Ative para Card 1.3 (sale processing) e Card 2.2 (settlement calculation) |
| **Um card de cada vez** | Espere um card completar antes de iniciar o dependente |
| **Paralelismo** | Cards sem dependência podem rodar com agentes concorrentes |
| **Review sempre** | Revise o código gerado antes de aprovar merge |
| **Contexto** | Anexe os docs relevantes ao criar cada card |
| **Cards focados** | Melhor ter mais cards pequenos do que poucos cards grandes |
| **Teste** | Peça ao agente para criar testes junto com a implementação |
