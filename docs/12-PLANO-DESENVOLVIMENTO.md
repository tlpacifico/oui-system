# Plano de Desenvolvimento - OUI System

**Última atualização:** 2026-02-12 18:45

Referência: [10-AUTOMAKER-GUIDE.md](10-AUTOMAKER-GUIDE.md) (Kanban Cards e ordem de execução).

---

## 🎯 Mudança importante: Autenticação com Firebase

O sistema foi atualizado para usar **Firebase Authentication** em vez do backend JWT customizado. Isso simplifica a gestão de utilizadores e adiciona funcionalidades como reset de password por email.

---

## Resumo do estado atual

| Fase | Descrição | Status |
|------|-----------|--------|
| **Fase 1** | POS (MVP) | Em progresso (Auth concluída) |
| **Fase 2** | Financeiro (Settlement, Store Credits) | Pendente |
| **Fase 3** | Reports & Dashboard | Pendente |
| **Fase 4** | Funcionalidades Extras | Pendente |

---

## Fase 1: POS (MVP)

| Card | Título | Status | Notas |
|------|--------|--------|--------|
| 1.1 | POS - Entidades do Banco de Dados | ✅ Concluído | CashRegister, Sale, SaleItem, SalePayment, ConsignmentItem; enums; migração AddPosEntities |
| 1.1a | Login com Firebase Authentication | ✅ Concluído | **Nova implementação:** Firebase Auth com @angular/fire, AuthService, interceptor (Firebase ID tokens), guard, login page (`/login`) com "Esqueci palavra-passe", rate limiting (5 tentativas = 15 min bloqueio) |
| 1.2 | POS - Cash Register Backend (open/close/status) | 🔄 Próximo | Comandos e endpoints |
| 1.3 | POS - Process Sale Backend | Pendente | Processamento de venda + comissão |
| 1.4 | POS - Frontend (Tela de Venda) | Pendente | Angular: pos-register, pos-sale, pos-payment-dialog |
| 1.5 | User Roles & Permissions (RBAC) | Pendente | Roles: Admin, Manager, Finance, Cashier (será integrado com Firebase Custom Claims) |
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
- **Firebase:** Autenticação configurada (apiKey, authDomain, projectId em `firebase.config.ts`), @angular/fire 20.0.1, firebase 11.10.0
- **Frontend:**
  - Login com Firebase Auth (`/login`) - AuthService com signals, rate limiting, password reset
  - HTTP interceptor para Firebase ID tokens
  - Auth guard protegendo rotas
  - Dashboard protegido (`/dashboard`) com logout

## Próximos passos

### Autenticação (concluída)
- ✅ Login page implementada com Firebase Authentication
- ✅ Rate limiting (5 tentativas = bloqueio 15 minutos)
- ✅ Password reset por email
- ✅ Auth guard e interceptor funcionais

### Próximas tarefas prioritárias

1. **Integração Firebase ↔ Backend:**
   - Configurar Firebase Admin SDK no backend .NET para validar tokens
   - Middleware para extrair Firebase UID dos tokens
   - Sincronizar utilizadores Firebase com tabela `Users` local
   - Mapear Firebase Custom Claims para Roles (Admin, Manager, Finance, Cashier)

2. **Card 1.2 - Cash Register Backend:**
   - Endpoints e CQRS para abrir/fechar caixa e consultar status
   - Comandos: OpenCashRegister, CloseCashRegister, GetCurrentCashRegister

3. **Card 1.3 - Process Sale Backend:**
   - Comando ProcessSale e endpoints de vendas
   - POST /api/pos/sales, GET /api/pos/sales/{id}
   - Cálculo de comissão automático

4. **Card 1.5 - RBAC:**
   - Implementar verificação de roles baseada em Firebase Custom Claims
   - Authorization policies no backend

5. **Card 1.4 - POS Frontend:**
   - Tela de venda (POS)
   - Carrinho de compras
   - Modal de pagamento
