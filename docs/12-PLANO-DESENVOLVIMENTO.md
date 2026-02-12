# Plano de Desenvolvimento - OUI System

**Última atualização:** 2026-02-12

Referência: [10-AUTOMAKER-GUIDE.md](10-AUTOMAKER-GUIDE.md) (Kanban Cards e ordem de execução).

---

## Resumo do estado atual

| Fase | Descrição | Status |
|------|-----------|--------|
| **Fase 1** | POS (MVP) | Em progresso |
| **Fase 2** | Financeiro (Settlement, Store Credits) | Pendente |
| **Fase 3** | Reports & Dashboard | Pendente |
| **Fase 4** | Funcionalidades Extras | Pendente |

---

## Fase 1: POS (MVP)

| Card | Título | Status | Notas |
|------|--------|--------|--------|
| 1.1 | POS - Entidades do Banco de Dados | ✅ Concluído | CashRegister, Sale, SaleItem, SalePayment, ConsignmentItem; enums; migração AddPosEntities |
| 1.1a | Login integrado com o Backend | ✅ Concluído | User entity, JWT (POST /api/auth/login), seed admin@oui.local / Admin123!; Angular: AuthService, interceptor, guard, login page ligada à API |
| 1.2 | POS - Cash Register Backend (open/close/status) | Pendente | Comandos e endpoints |
| 1.3 | POS - Process Sale Backend | Pendente | Processamento de venda + comissão |
| 1.4 | POS - Frontend (Tela de Venda) | Pendente | Angular: pos-register, pos-sale, pos-payment-dialog |
| 1.5 | User Roles & Permissions (RBAC) | Pendente | Roles: Admin, Manager, Finance, Cashier |
| 1.6 | Barcode & Label Printing | Pendente | Etiquetas e código de barras |

**Ordem recomendada:** 1.1 → 1.2 → 1.3 | 1.5 em paralelo → 1.4 → 1.6 em paralelo.

---

## Fase 2: Financeiro

| Card | Título | Status |
|------|--------|--------|
| 2.1 | Settlement - Entidades | Pendente |
| 2.2 | Settlement - Backend | Pendente |
| 2.3 | Settlement - Frontend | Pendente |
| 2.4 | Store Credits | Pendente |

---

## Fase 3: Reports & Dashboard

| Card | Título | Status |
|------|--------|--------|
| 3.1 | Dashboard API | Pendente |
| 3.2 | Reports API | Pendente |
| 3.3 | Dashboard & Reports Frontend | Pendente |

---

## Fase 4: Funcionalidades Extras

| Card | Título | Status |
|------|--------|--------|
| 4.1 | Supplier Portal | Pendente |
| 4.2 | Promotions Engine | Pendente |
| 4.3 | Customer & Loyalty | Pendente |
| 4.4 | Inventory Alerts & Notifications | Pendente |
| 4.5 | Audit Log | Pendente |

---

## Dependências técnicas já existentes

- .NET 9, Angular 20, PostgreSQL (connection string em appsettings; rodar migração com `dotnet ef database update --project src/shs.Infrastructure --startup-project src/shs.Api`)
- Estrutura: shs.Api, shs.Application, shs.Domain, shs.Infrastructure
- **Domain:** entidades base (`EntityWithIdAuditable`, `IHaveSoftDelete`), entidades POS e `ConsignmentItem`, entidade `User` (login), enums (CashRegisterStatus, SaleStatus, PaymentMethodType, ConsignmentItemStatus)
- **Infrastructure:** `ShsDbContext`, configurações EF para todas as entidades, migrações `AddPosEntities` e `AddUsers`, `AddInfrastructure()` com PostgreSQL
- **API:** JWT (appsettings Jwt:*), CORS, POST /api/auth/login; seed utilizador admin@oui.local (palavra-passe Admin123!)
- Frontend: login integrado com a API (AuthService, interceptor, guard), dashboard protegido e com logout

## Próximos passos

1. **Aplicar migração Users** (se ainda não aplicada): `dotnet ef database update --project src/shs.Infrastructure --startup-project src/shs.Api`
2. **Card 1.2:** Endpoints e CQRS para abrir/fechar caixa e consultar status (OpenCashRegister, CloseCashRegister, GetCurrentCashRegister, etc.).
3. **Card 1.3:** Comando ProcessSale e endpoints de vendas (POST /api/pos/sales, GET /api/pos/sales/{id}, etc.).
4. **Card 1.5:** RBAC (em paralelo quando fizer sentido).
