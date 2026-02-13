# Plano de Desenvolvimento - OUI System

**Última atualização:** 2026-02-13 00:05

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
| **Fase 1** | Inventário & Consignações (M1 + M2) | 🔄 Próxima prioridade |
| **Fase 2** | POS (M3) | Pendente |
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
| 1.1.2 | CU-01: Registar Peça no Inventário | Pendente | CU-01 | Backend: comando CreateConsignmentItem, validações, geração de ID automático |
| 1.1.3 | CU-06: Registar Peça de Compra Própria | Pendente | CU-06 | Backend: comando CreateOwnPurchaseItem, sem comissão, origem (Humana, Vinted, etc.) |
| 1.1.4 | CU-02: Pesquisar/Consultar Inventário | Pendente | CU-02 | Backend: Query com filtros (nome, marca, preço, tamanho, fornecedor, estado, origem) |
| 1.1.5 | CU-03: Atualizar Preço da Peça | Pendente | CU-03 | Backend: comando UpdateItemPrice, auditoria de mudanças |
| 1.1.6 | CU-05: Eliminar Peça | Pendente | CU-05 | Backend: soft-delete, validação (não pode estar vendida) |
| 1.1.7 | Frontend - Lista de Peças | Pendente | CU-02 | Angular: página `/inventory/items` com filtros, tabela, paginação (PG-03) |
| 1.1.8 | Frontend - Detalhe da Peça | Pendente | - | Angular: página `/inventory/items/:id` com todas as info (PG-04) |
| 1.1.9 | Frontend - Cadastro/Edição de Peça | Pendente | CU-01, CU-06 | Angular: formulário `/inventory/items/:id/edit` (PG-05) |
| 1.1.10 | Gestão de Marcas | ✅ Concluído | - | Backend: CRUD `/api/brands`; Frontend: `/inventory/brands` com modal criar/editar/eliminar (PG-06) |
| 1.1.11 | Gestão de Tags/Categorias | ✅ Concluído | - | Backend: CRUD `/api/categories` e `/api/tags`; Frontend: `/inventory/categories` (hierarquia) e `/inventory/tags` (grid com color picker) (PG-07) |
| 1.1.12 | CU-07: Registar Fornecedor | 🔄 Próximo | CU-07 | Backend + Frontend: CRUD de fornecedores (PG-14) |

### 1.2 - Gestão de Consignações (M2)

| Card | Título | Status | Use Case | Notas |
|------|--------|--------|----------|--------|
| 1.2.1 | Consignação - Entidades & Banco de Dados | Pendente | Base | Entidades: Supplier, Reception, ConsignmentItem (já existe), Settlement; estados do fluxo |
| 1.2.3 | CU-08: Recepção de Peças (Etapa 1) | Pendente | CU-08 | Backend: comando CreateReception, gerar recibo PDF, apenas contagem |
| 1.2.4 | CU-09: Avaliar Peças (Etapa 2) | Pendente | CU-09 | Backend: avaliação individual de cada peça, marcar defeitos |
| 1.2.5 | CU-10: Enviar Email de Avaliação (Etapa 3) | Pendente | CU-10 | Backend: template de email, lista aceites/recusadas, envio SMTP |
| 1.2.6 | Frontend - Lista de Fornecedores | Pendente | - | Angular: `/suppliers` (PG-12) |
| 1.2.7 | Frontend - Detalhe do Fornecedor | Pendente | - | Angular: `/suppliers/:id` com tabs (PG-13) |
| 1.2.8 | Frontend - Recepção de Peças | Pendente | CU-08 | Angular: `/consignments/receive` (PG-10) |
| 1.2.9 | Frontend - Avaliações Pendentes | Pendente | - | Angular: `/consignments/pending-evaluations` (PG-NEW-1) |
| 1.2.10 | Frontend - Avaliar Recepção | Pendente | CU-09, CU-10 | Angular: `/consignments/:id/evaluate` (PG-NEW-2) |
| 1.2.11 | Frontend - Detalhe da Consignação | Pendente | - | Angular: `/consignments/:id` (PG-11) |
| 1.2.12 | CU-14: Devolver Peças ao Fornecedor | Pendente | CU-14 | Backend + Frontend: `/consignments/returns` (PG-15) |

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
| 2.2 | POS - Cash Register Backend (open/close/status) | Pendente | Comandos e endpoints |
| 2.3 | POS - Process Sale Backend | Pendente | Processamento de venda + comissão |
| 2.4 | POS - Frontend (Tela de Venda) | Pendente | Angular: pos-register, pos-sale, pos-payment-dialog |
| 2.5 | User Roles & Permissions (RBAC) | Pendente | Roles: Admin, Manager, Finance, Cashier (será integrado com Firebase Custom Claims) |
| 2.6 | Barcode & Label Printing | Pendente | Etiquetas e código de barras |

---

## Fase 3: Financeiro (M4)

| Card | Título | Status |
|------|--------|--------|
| 3.1 | Settlement - Entidades | Pendente |
| 3.2 | Settlement - Backend | Pendente |
| 3.3 | Settlement - Frontend | Pendente |
| 3.4 | Store Credits | Pendente |

---

## Fase 4: Reports & Dashboard (M5)

| Card | Título | Status |
|------|--------|--------|
| 4.1 | Dashboard API | Pendente |
| 4.2 | Reports API | Pendente |
| 4.3 | Dashboard & Reports Frontend | Pendente |

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

4. **Card 1.1.12 - CU-07: Registar Fornecedor**
   - Backend: CRUD de fornecedores
   - Validações: NIF português, telefone +351, inicial única
   - Endpoints: `/api/suppliers`
   - Pré-requisito para registar peças de consignação

5. **Card 1.1.2 - CU-01: Registar Peça (Consignação)**
   - Backend: comando `CreateConsignmentItemCommand`
   - Validações: nome, marca obrigatória, preço > 0
   - Geração automática de ID: `{Inicial}{YYYYMM}{Sequência:0000}`
   - Endpoint: `POST /api/inventory/items/consignment`

6. **Card 1.1.7 - Frontend: Lista de Peças**
   - Angular: página `/inventory/items` (PG-03)
   - Tabela com foto, ID, nome, marca, tamanho, preço, estado
   - Filtros básicos (pesquisa por texto, marca, estado)
   - Paginação (20 itens por página)

7. **Card 1.1.8 - Frontend: Detalhe da Peça**
   - Angular: página `/inventory/items/:id` (PG-04)
   - Mostrar todas as informações da peça
   - Galeria de fotos
   - Histórico de alterações

8. **Card 1.1.9 - Frontend: Cadastro/Edição de Peça**
   - Angular: formulário `/inventory/items/:id/edit` (PG-05)
   - Formulário completo com todos os campos
   - Upload de fotos
   - Validações client-side

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

12. **Card 1.2.8 - CU-08: Recepção de Peças (Etapa 1)**
   - Backend: comando `CreateReceptionCommand`
   - Gerar recibo PDF (sem valores)
   - Endpoint: `POST /api/consignments/receive`
   - Frontend: `/consignments/receive` (PG-10)

#### Sprint 3: Fluxo de Avaliação

13. **Card 1.2.9 - Frontend: Avaliações Pendentes**
    - Angular: `/consignments/pending-evaluations` (PG-NEW-1)
    - Lista de recepções aguardando avaliação

14. **Card 1.2.4 - CU-09: Avaliar Peças (Etapa 2)**
    - Backend: avaliação individual de peças
    - Marcar defeitos
    - Endpoint: `PUT /api/consignments/{id}/evaluate`

15. **Card 1.2.10 - Frontend: Avaliar Recepção**
    - Angular: `/consignments/:id/evaluate` (PG-NEW-2)
    - Formulário para avaliar cada peça
    - Opção "Com Defeito"

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
