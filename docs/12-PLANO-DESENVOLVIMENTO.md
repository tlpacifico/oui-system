# Plano de Desenvolvimento - OUI System

**Última atualização:** 2026-02-14 (Reports API e Frontend implementados)

Referência: [10-AUTOMAKER-GUIDE.md](10-AUTOMAKER-GUIDE.md) (Kanban Cards e ordem de execução).

---

## 🎯 Mudanças importantes

### Autenticação com Firebase
O sistema foi atualizado para usar **Firebase Authentication** em vez do backend JWT customizado. Isso simplifica a gestão de utilizadores e adiciona funcionalidades como reset de password por email.

### ⚡ Nova Prioridade: Inventário First
A ordem de implementação foi ajustada para começar pelo **Módulo de Inventário (M1)** em vez do POS. Esta abordagem permite:
- Construir a base de dados de produtos primeiro
- Ter peças catalogadas antes de vender
- Implementar o fluxo completo de consignação (recepção → avaliação → venda)

---

## Resumo do estado atual

| Fase | Descrição | Status |
|------|-----------|--------|
| **Fase 0** | Autenticação | ✅ Concluída |
| **Fase 1** | Inventário & Consignações (M1 + M2) | ✅ Concluída |
| **Fase 2** | POS (M3) + RBAC | ✅ Concluída |
| **Fase 3** | Financeiro (M4) | Pendente |
| **Fase 4** | Reports & Dashboard (M5) | Pendente |
| **Fase 5** | Funcionalidades Extras | Pendente |

---

## Fase 0: Autenticação ✅

| Card | Título | Status | Notas |
|------|--------|--------|--------|
| 0.1 | Login com Firebase Authentication | ✅ Concluído | Firebase Auth com @angular/fire, AuthService, interceptor (Firebase ID tokens), guard, login page (`/login`) com "Esqueci palavra-passe", rate limiting (5 tentativas = 15 min bloqueio) |

---

## Fase 1: Inventário & Consignações (M1 + M2) - PRIORIDADE

### 1.1 - Gestão de Inventário (M1)

| Card | Título | Status | Use Case | Notas |
|------|--------|--------|----------|--------|
| 1.1.1 | Inventário - Entidades & Banco de Dados | ✅ Concluído | Base | Entidades: Item, Brand, Category, Tag, ItemPhoto; enums (ItemStatus, ItemCondition, ItemOrigin); migração |
| 1.1.2 | CU-01: Registar Peça no Inventário | ✅ Concluído | CU-01 | Backend: `POST /api/inventory/items` e `POST /api/inventory/items/consignment`, validações, geração de ID automático |
| 1.1.3 | CU-06: Registar Peça de Compra Própria | ✅ Concluído | CU-06 | Integrado no endpoint `POST /api/inventory/items` com AcquisitionType=OwnPurchase, sem comissão, origem (Humana, Vinted, etc.) |
| 1.1.4 | CU-02: Pesquisar/Consultar Inventário | ✅ Concluído | CU-02 | Backend: `GET /api/inventory/items` com filtros (nome, marca, estado), paginação |
| 1.1.5 | CU-03: Atualizar Preço da Peça | ✅ Concluído | CU-03 | Integrado no endpoint `PUT /api/inventory/items/{id}` |
| 1.1.6 | CU-05: Eliminar Peça | ✅ Concluído | CU-05 | Backend: `DELETE /api/inventory/items/{id}` soft-delete, validação (não pode estar vendida) |
| 1.1.7 | Frontend - Lista de Peças | ✅ Concluído | CU-02 | Angular: `/inventory/items` com filtros, tabela, paginação, badges de estado, alertas de dias em stock (PG-03) |
| 1.1.8 | Frontend - Detalhe da Peça | ✅ Concluído | - | Angular: `/inventory/items/:id` com KPIs, galeria de fotos, info completa, tags, rejeição (PG-04) |
| 1.1.9 | Frontend - Cadastro/Edição de Peça | ✅ Concluído | CU-01, CU-06 | Angular: `/inventory/items/new` e `/inventory/items/:id/edit` com formulário completo, dropdowns de marca/categoria/fornecedor/tags, validações client-side (PG-05) |
| 1.1.10 | Gestão de Marcas | ✅ Concluído | - | Backend: CRUD `/api/brands`; Frontend: `/inventory/brands` com modal criar/editar/eliminar (PG-06) |
| 1.1.11 | Gestão de Tags/Categorias | ✅ Concluído | - | Backend: CRUD `/api/categories` e `/api/tags`; Frontend: `/inventory/categories` (hierarquia) e `/inventory/tags` (grid com color picker) (PG-07) |
| 1.1.12 | CU-07: Registar Fornecedor | ✅ Concluído | CU-07 | Backend: CRUD `/api/suppliers` com validações (NIF, telefone +351, inicial única); Frontend: `/inventory/suppliers` com modal criar/editar/eliminar (PG-14) |
| 1.1.13 | Fotos de Itens (Upload/Gestão) | ✅ Concluído | - | Backend: `POST /api/inventory/items/{id}/photos` (upload multifile), `DELETE /photos/{photoId}`, `PUT /photos/reorder`; Ficheiros em `wwwroot/uploads/items/{id}/`; Frontend: drag & drop upload, eliminar, galeria no detalhe; Máx 10 fotos, 10 MB, JPEG/PNG/WebP |

### 1.2 - Gestão de Consignações (M2)

| Card | Título | Status | Use Case | Notas |
|------|--------|--------|----------|--------|
| 1.2.1 | Consignação - Entidades & Banco de Dados | ✅ Concluído | Base | Entidades Supplier, Reception, Item já existem; enums ReceptionStatus; migração AddInventoryEntities |
| 1.2.3 | CU-08: Recepção de Peças (Etapa 1) | ✅ Concluído | CU-08 | Backend: `POST /api/consignments/receptions` (criar), `GET` (listar/detalhe), recibo HTML imprimível; Frontend: `/consignments/receptions` (lista) e `/consignments/receive` (formulário) |
| 1.2.4 | CU-09: Avaliar Peças (Etapa 2) | ✅ Concluído | CU-09 | Backend: `POST /receptions/{id}/items` (adicionar item), `GET /receptions/{id}/items` (listar), `DELETE /receptions/{id}/items/{itemId}` (remover), `PUT /receptions/{id}/complete-evaluation` (concluir); Frontend: `/consignments/receptions/:id/evaluate` (formulário de avaliação individual) |
| 1.2.5 | CU-10: Enviar Email de Avaliação (Etapa 3) | ✅ Concluído | CU-10 | Backend: `IEmailService` + `EmailService` (MailKit/SMTP), template HTML profissional com peças aceites/recusadas, envio automático ao concluir avaliação, `POST /receptions/{id}/send-evaluation-email` para reenvio; Frontend: botão enviar/reenviar email na página de avaliação; Config `Smtp` em `appsettings.json` |
| 1.2.6 | Frontend - Lista de Fornecedores | ✅ Concluído | - | Angular: `/inventory/suppliers` com CRUD, pesquisa, modal criar/editar/eliminar, link para detalhe (PG-12) |
| 1.2.7 | Frontend - Detalhe do Fornecedor | ✅ Concluído | - | Angular: `/inventory/suppliers/:id` com tabs Info/Itens/Recepções, KPIs, paginação de itens (PG-13); Backend: `GET /api/suppliers/{id}/items` e `/receptions` |
| 1.2.8 | Frontend - Recepção de Peças | ✅ Concluído | CU-08 | Angular: `/consignments/receive` (PG-10), `/consignments/receptions` (lista), sidebar atualizado |
| 1.2.9 | Frontend - Avaliações Pendentes | ✅ Concluído | - | Angular: `/consignments/pending-evaluations` com cards de progresso, sidebar atualizado (PG-NEW-1) |
| 1.2.10 | Frontend - Avaliar Recepção | ✅ Concluído | CU-09, CU-10 | Angular: `/consignments/receptions/:id/evaluate` com formulário por peça, barra de progresso, aceitar/rejeitar, concluir avaliação (PG-NEW-2) |
| 1.2.11 | Frontend - Detalhe da Consignação | ✅ Concluído | - | Angular: `/consignments/receptions/:id` (PG-11) com header, KPIs (total/avaliadas/aceites/rejeitados), info da recepção, resumo financeiro (valor total, comissão média, valor fornecedor), tabela de peças com links, timeline, ações (recibo, avaliar, enviar email); Botão "Ver" na lista de recepções |
| 1.2.12 | CU-14: Devolver Peças ao Fornecedor | ✅ Concluído | CU-14 | Backend: SupplierReturnEntity + migration, endpoints (GET/POST returnable-items, returns CRUD); Frontend: `/consignments/returns` (lista paginada com pesquisa), `/consignments/returns/new` (seleção fornecedor + itens com checkboxes, confirmação), `/consignments/returns/:id` (detalhe com KPIs, tabela, timeline); Sidebar "Devoluções" adicionado |

**Ordem recomendada de implementação:**
1. **Dados base (pré-requisitos):** 1.1.1 → 1.1.10 → 1.1.11 → 1.1.12
2. **Inventário básico:** 1.1.2 → 1.1.7 → 1.1.8 → 1.1.9
3. **Fornecedores:** 1.2.1 → 1.2.6 → 1.2.7
4. **Fluxo de Consignação:** 1.2.3 → 1.2.4 → 1.2.5 → 1.2.8 → 1.2.9 → 1.2.10
5. **Funcionalidades complementares:** 1.1.3, 1.1.4, 1.1.5, 1.1.6, 1.2.11, 1.2.12

---

## Fase 2: POS (M3)

| Card | Título | Status | Notas |
|------|--------|--------|--------|
| 2.1 | POS - Entidades do Banco de Dados | ✅ Concluído | CashRegister, Sale, SaleItem, SalePayment; enums; migração AddPosEntities |
| 2.2 | POS - Cash Register Backend (open/close/status) | ✅ Concluído | Endpoints: `POST /api/pos/register/open` (validação 1 caixa por operador), `POST /close` (cálculo automático de valor esperado, discrepância, totais por método pagamento), `GET /current` (caixa aberta do utilizador), `GET /{id}` (detalhe com vendas), `GET /status` (monitorização de todas as caixas); Claims do Firebase para identificar operador |
| 2.3 | POS - Process Sale Backend | ✅ Concluído | `POST /api/pos/sales` com validação completa (caixa aberta, itens ToSell, pagamentos >= total, máx 2 métodos), cálculo de preços (subtotal, desconto %, desconto por item, total), geração de SaleNumber V{YYYYMMDD}-{seq:000}, actualização item→Sold com FinalSalePrice e CommissionAmount, cálculo de troco; `GET /sales/{id}` detalhe com itens/pagamentos; `GET /sales/today` resumo (count, revenue, avg ticket, por método); `GET /sales` pesquisa paginada com filtros data |
| 2.4 | POS - Frontend (Tela de Venda) | ✅ Concluído | PosService com todos os endpoints; `/pos` - Caixa (abrir com valor inicial, fechar com contagem e sumário de discrepância, KPIs, ações rápidas); `/pos/sale` - Tela de venda full-width (pesquisa de itens ToSell à esquerda, carrinho à direita, desconto global %, dialog de pagamento com troco, atalhos F4/ESC); `/pos/sales` - Vendas de hoje (KPIs, breakdown por método pagamento, tabela recente); Sidebar "Vendas" com Caixa, Nova Venda, Vendas de Hoje |
| 2.5 | User Roles & Permissions (RBAC) | ✅ Concluído | **Backend:** 4 entidades (Role, Permission, UserRole, RolePermission), migração AddRBACEntities, RbacSeedService (28 permissões, 4 roles: Admin/Manager/Cashier/Inventory Clerk), PermissionAuthorizationHandler (lookup de permissões por email de Firebase token), endpoints CRUD (`/api/roles`, `/api/permissions`, `/api/roles/{id}/permissions`, `/api/users/{id}/roles`, `/api/me/roles`, `/api/me/permissions`), todos endpoints protegidos com `.RequirePermission()`; **Frontend:** AuthService com `loadUserAuthContext()`, permissionGuard, `*hasPermission` e `*hasRole` directives, páginas admin (`/admin/roles`, `/admin/roles/:id`, `/admin/permissions`), sidebar "Administração" com visibilidade condicional; Integração com Firebase Auth mantida (email lookup para carregar permissões) |
| 2.6 | Barcode & Label Printing | Pendente | Etiquetas e código de barras |

---

## Fase 3: Financeiro (M4)

| Card | Título | Status |
|------|--------|--------|
| 3.1 | Settlement - Entidades | ✅ Concluído | SettlementEntity, SaleItem.SettlementId, StoreCredit, SupplierCashBalanceTransaction; PorcInLoja + PorcInDinheiro |
| 3.2 | Settlement - Backend | ✅ Concluído | Endpoints: GET pending-items, POST calculate, POST create, GET list, GET by id, POST process-payment, DELETE cancel |
| 3.3 | Settlement - Frontend | ✅ Concluído | `/finance/settlements` (lista com tabs Pendentes/Processados/Todos), `/finance/settlements/new` (criar acerto), `/finance/settlements/:id` (detalhe, processar pagamento, cancelar); Sidebar "Financeiro" > "Acertos" |
| 3.4 | Store Credits | ✅ Concluído | Frontend: `/finance/credits` (seleção fornecedor, créditos em loja, saldo resgate, emitir crédito, processar resgate); `/finance/credits/:id` (detalhe, ajustar, cancelar); Sidebar "Créditos em Loja" |

---

## Fase 4: Reports & Dashboard (M5)

| Card | Título | Status |
|------|--------|--------|
| 4.1 | Dashboard API | ✅ Concluído | GET /api/dashboard?period=today\|week\|month: salesToday, salesMonth, inventory, pendingSettlements, topSellingItems, alerts, salesChart |
| 4.2 | Reports API | ✅ Concluído | GET /api/reports/sales, /inventory, /suppliers, /finance com filtros de período |
| 4.3 | Dashboard & Reports Frontend | ✅ Concluído | Dashboard + Relatórios (Vendas, Inventário, Fornecedores, Financeiro); Sidebar "Relatórios" |

---

## Fase 5: Funcionalidades Extras

| Card | Título | Status |
|------|--------|--------|
| 5.1 | Supplier Portal | Pendente |
| 5.2 | Promotions Engine | Pendente |
| 5.3 | Customer & Loyalty | Pendente |
| 5.4 | Inventory Alerts & Notifications | Pendente |
| 5.5 | Audit Log | Pendente |

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

### ✅ Fase 0: Autenticação (Concluída)
- ✅ Login page implementada com Firebase Authentication
- ✅ Rate limiting (5 tentativas = bloqueio 15 minutos)
- ✅ Password reset por email
- ✅ Auth guard e interceptor funcionais

### 🔄 Fase 1: Inventário & Consignações (PRÓXIMA PRIORIDADE)

#### Sprint 1: Dados Base & Inventário Básico
**Objetivo:** Criar pré-requisitos (marcas, categorias, fornecedores) e permitir cadastro e listagem de peças

1. **Card 1.1.1 - Inventário: Entidades & Banco de Dados**
   - Criar entidades: `Item`, `Brand`, `Category`, `Tag`, `ItemPhoto`
   - Enums: `ItemStatus`, `ItemCondition`, `ItemOrigin`, `AcquisitionType`
   - Configurações EF Core
   - Migração `AddInventoryEntities`
   - Seed data: marcas e categorias iniciais

2. **Card 1.1.10 - Gestão de Marcas**
   - Backend + Frontend: CRUD de marcas (PG-06)
   - Pré-requisito para registar peças (marca obrigatória)

3. **Card 1.1.11 - Gestão de Tags/Categorias**
   - Backend + Frontend: CRUD de categorias e tags (PG-07)

4. **Card 1.1.12 - CU-07: Registar Fornecedor** ✅
   - Backend: CRUD `/api/suppliers` com validações (NIF português mod 11, telefone +351XXXXXXXXX, inicial única)
   - Frontend: `/inventory/suppliers` com modal criar/editar/eliminar
   - Sidebar atualizado com link ativo para Fornecedores

5. **Card 1.1.2 - CU-01: Registar Peça (Consignação)** ✅
   - Backend: `POST /api/inventory/items` (geral) e `POST /api/inventory/items/consignment` (via recepção)
   - Validações: nome, marca obrigatória, preço > 0
   - Geração automática de ID: `{Inicial}{YYYYMM}-{Sequência:0000}`

6. **Card 1.1.7 - Frontend: Lista de Peças** ✅
   - Angular: `/inventory/items` com filtros, tabela, paginação, badges, alertas de dias

7. **Card 1.1.8 - Frontend: Detalhe da Peça** ✅
   - Angular: `/inventory/items/:id` com KPIs, fotos, info completa, tags

8. **Card 1.1.9 - Frontend: Cadastro/Edição de Peça** ✅
   - Angular: `/inventory/items/new` (criar) e `/inventory/items/:id/edit` (editar)
   - Formulário com dropdowns de marca, categoria, fornecedor, tags
   - Suporte para Consignação e Compra Própria
   - Backend: `PUT /api/inventory/items/{id}` e `DELETE /api/inventory/items/{id}` adicionados

#### Sprint 2: Consignações & Recepção

9. **Card 1.2.1 - Consignação: Entidades & Banco de Dados**
   - Criar entidades: `Supplier`, `Reception`
   - Atualizar `ConsignmentItem` (já existe)
   - Migração `AddConsignmentEntities`

10. **Card 1.2.6 - Frontend: Lista de Fornecedores**
    - Angular: `/suppliers` (PG-12)
    - Tabela com pesquisa

11. **Card 1.2.7 - Frontend: Detalhe do Fornecedor**
    - Angular: `/suppliers/:id` com tabs (PG-13)

12. **Card 1.2.3 + 1.2.8 - CU-08: Recepção de Peças (Etapa 1)** ✅
   - Backend: `POST /api/consignments/receptions` (criar recepção), `GET` (listar com filtros e paginação, detalhe, recibo HTML)
   - Frontend: `/consignments/receive` (formulário com seleção de fornecedor, contagem, notas) e `/consignments/receptions` (lista com filtros)
   - Recibo imprimível em HTML (sem valores, apenas contagem, assinaturas)
   - Sidebar atualizado com links para Recepções e Nova Recepção

#### Sprint 3: Fluxo de Avaliação

13. **Card 1.2.9 - Frontend: Avaliações Pendentes** ✅
    - Angular: `/consignments/pending-evaluations` (PG-NEW-1)
    - Cards com info do fornecedor, contagem, barra de progresso, link para avaliar

14. **Card 1.2.4 - CU-09: Avaliar Peças (Etapa 2)** ✅
    - Backend: `POST /api/consignments/receptions/{id}/items` (adicionar item avaliado)
    - `GET /api/consignments/receptions/{id}/items` (listar itens avaliados)
    - `DELETE /api/consignments/receptions/{id}/items/{itemId}` (remover item)
    - `PUT /api/consignments/receptions/{id}/complete-evaluation` (concluir, muda status para Evaluated, itens aceites → ToSell)

15. **Card 1.2.10 - Frontend: Avaliar Recepção** ✅
    - Angular: `/consignments/receptions/:id/evaluate` (PG-NEW-2)
    - Barra de progresso, tabela de itens avaliados, formulário inline para cada peça
    - Aceitar ou rejeitar (com motivo), remover item, concluir avaliação
    - Sidebar atualizado com link "Avaliações Pendentes"

16. **Card 1.2.5 - CU-10: Enviar Email de Avaliação**
    - Backend: template de email
    - Integração SMTP
    - Envio automático após conclusão

### 🔮 Backlog Futuro

**Integração Firebase ↔ Backend:**
- Configurar Firebase Admin SDK no backend .NET
- Middleware para validar Firebase tokens
- Sincronizar utilizadores com PostgreSQL
- Firebase Custom Claims para RBAC

**Inventário - Funcionalidades Complementares:**
- CU-06: Peça de Compra Própria
- CU-03: Atualizar Preço
- CU-05: Eliminar Peça
- Gestão de Marcas e Categorias
- Alertas de stock parado
