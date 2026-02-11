# SHS - Mapeamento Páginas x Endpoints

## Versão: 1.0 | Última Atualização: 2026-02-11

Este documento mapeia quais endpoints da API são consumidos por cada página do sistema.

---

## Legenda

- **R** = Requisição feita no carregamento da página (Read)
- **A** = Requisição feita por ação do usuário (Action)
- **P** = Requisição periódica/polling

---

## 1. Autenticação

### PG-01: Login (`/login`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/login` | POST | A | Ao clicar "Entrar" |

---

## 2. Core

### PG-02: Dashboard (`/dashboard`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/me` | GET | R | Carregar perfil do usuário |
| `/dashboard/kpis` | GET | R/P | Carregar KPIs (auto-refresh 5min) |
| `/dashboard/sales-chart` | GET | R | Carregar gráfico de vendas |
| `/dashboard/alerts` | GET | R/P | Carregar alertas pendentes |
| `/dashboard/top-sales` | GET | R | Carregar top 5 vendas |
| `/notifications/unread-count` | GET | P | Badge de notificações (polling 1min) |

---

## 3. Inventário

### PG-03: Lista de Itens (`/inventory/items`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/items` | GET | R/A | Carregar lista, aplicar filtros, paginar |
| `/brands` | GET | R | Popular dropdown de marcas |
| `/tags` | GET | R | Popular dropdown de tags |
| `/categories` | GET | R | Popular dropdown de categorias |
| `/suppliers` | GET | R | Popular dropdown de fornecedores |
| `/items/:id` | DELETE | A | Excluir item |
| `/items/batch/labels` | POST | A | Imprimir etiquetas em lote |

### PG-04: Detalhe do Item (`/inventory/items/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/items/:id` | GET | R | Carregar dados do item |
| `/items/:id/price-history` | GET | R | Carregar histórico de preços |
| `/items/:id/history` | GET | R | Carregar log de ações |
| `/items/:id` | DELETE | A | Excluir item |
| `/items/batch/labels` | POST | A | Imprimir etiqueta |

### PG-05: Cadastro/Edição de Item (`/inventory/items/:id/edit`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/items/:id` | GET | R | Carregar dados (modo edição) |
| `/items` | POST | A | Salvar novo item |
| `/items/:id` | PUT | A | Atualizar item existente |
| `/items/:id/price` | PATCH | A | Alterar preço |
| `/items/:id/photos` | POST | A | Upload de fotos |
| `/items/:id/photos/:photoId` | DELETE | A | Remover foto |
| `/brands` | GET | R | Popular autocomplete de marcas |
| `/tags` | GET | R | Popular autocomplete de tags |
| `/categories` | GET | R | Popular dropdown de categorias |

### PG-06: Marcas (`/inventory/brands`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/brands` | GET | R | Listar marcas |
| `/brands` | POST | A | Criar marca |
| `/brands/:id` | PUT | A | Editar marca |
| `/brands/:id` | DELETE | A | Excluir marca |

### PG-07: Tags/Categorias (`/inventory/tags`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/tags` | GET | R | Listar tags |
| `/tags` | POST | A | Criar tag |
| `/tags/:id` | PUT | A | Editar tag |
| `/tags/:id` | DELETE | A | Excluir tag |
| `/categories` | GET | R | Listar categorias |
| `/categories` | POST | A | Criar categoria |
| `/categories/:id` | PUT | A | Editar categoria |
| `/categories/:id` | DELETE | A | Excluir categoria |

### PG-08: Alertas de Estoque (`/inventory/alerts`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/inventory/alerts` | GET | R | Listar alertas com filtro de nível |
| `/inventory/alerts/summary` | GET | R | Contagem por nível |
| `/items/:id/price` | PATCH | A | Reduzir preço de item |
| `/consignments/:id/return` | PATCH | A | Devolver item ao fornecedor |
| `/consignments/:id/renew` | PATCH | A | Renovar consignação |

---

## 4. Consignações

### PG-09: Lista de Consignações (`/consignments`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/consignments` | GET | R/A | Listar com filtros e paginação |
| `/suppliers` | GET | R | Popular filtro de fornecedores |

### PG-10: Nova Consignação (`/consignments/new`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Popular autocomplete de fornecedores |
| `/suppliers` | POST | A | Cadastrar fornecedor inline (se novo) |
| `/consignments` | POST | A | Criar consignação |
| `/consignments/:id/items` | POST | A | Adicionar cada item |
| `/brands` | GET | R | Popular autocomplete de marcas |
| `/tags` | GET | R | Popular autocomplete de tags |
| `/categories` | GET | R | Popular dropdown de categorias |
| `/consignments/:id/contract` | GET | A | Gerar contrato PDF |

### PG-11: Detalhe da Consignação (`/consignments/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/consignments/:id` | GET | R | Carregar dados da consignação |
| `/consignments/:id/items` | POST | A | Adicionar item |
| `/consignments/:id/renew` | PATCH | A | Renovar |
| `/consignments/:id/return` | PATCH | A | Devolver itens |
| `/consignments/:id/contract` | GET | A | Gerar/imprimir contrato PDF |

### PG-12: Lista de Fornecedores (`/suppliers`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Listar fornecedores |
| `/suppliers/:id` | DELETE | A | Excluir fornecedor |

### PG-13: Detalhe do Fornecedor (`/suppliers/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers/:id` | GET | R | Dados do fornecedor |
| `/suppliers/:id/consignments` | GET | R | Consignações do fornecedor |
| `/suppliers/:id/items` | GET | R | Itens do fornecedor |
| `/suppliers/:id/settlements` | GET | R | Acertos do fornecedor |
| `/suppliers/:id/stats` | GET | R | Estatísticas |

### PG-14: Cadastro/Edição de Fornecedor (`/suppliers/:id/edit`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers/:id` | GET | R | Carregar dados (modo edição) |
| `/suppliers` | POST | A | Criar fornecedor |
| `/suppliers/:id` | PUT | A | Atualizar fornecedor |

---

## 5. Ponto de Venda (POS)

### PG-16: POS - Caixa (`/pos`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Verificar se caixa está aberto |
| `/items` | GET | A | Buscar item por barcode/nome |
| `/customers/search` | GET | A | Buscar cliente fidelidade |
| `/pos/sales` | POST | A | Processar venda |
| `/pos/sales/:id/receipt` | GET | A | Gerar recibo para impressão |
| `/finance/credits/search` | GET | A | Buscar crédito em loja (pagamento) |
| `/promotions/active` | GET | R | Carregar promoções ativas |

### PG-17: POS - Abrir Caixa (`/pos/open`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Verificar se já tem caixa aberto |
| `/pos/register/open` | POST | A | Abrir caixa |

### PG-18: POS - Fechar Caixa (`/pos/close`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Carregar resumo do caixa |
| `/pos/sales/today` | GET | R | Resumo de vendas do dia |
| `/pos/register/close` | POST | A | Fechar caixa |

### PG-19: Vendas do Dia (`/pos/sales`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales` | GET | R/A | Listar vendas com filtros |
| `/pos/sales/today` | GET | R | Totalizadores do dia |

### PG-20: Detalhe da Venda (`/pos/sales/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales/:id` | GET | R | Carregar dados da venda |
| `/pos/sales/:id/receipt` | GET | A | Gerar/imprimir recibo |
| `/pos/sales/:id/void` | POST | A | Cancelar venda |

### PG-21: Devoluções (`/pos/returns`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/returns` | GET | R | Listar devoluções |

### PG-22: Nova Devolução (`/pos/returns/new`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales` | GET | A | Buscar venda original |
| `/pos/sales/:id` | GET | A | Carregar detalhes da venda |
| `/pos/returns` | POST | A | Processar devolução |

---

## 6. Financeiro

### PG-23: Acertos com Fornecedores (`/finance/settlements`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/settlements` | GET | R | Listar acertos |
| `/finance/settlements/pending` | GET | R | Acertos pendentes |

### PG-24: Novo Acerto (`/finance/settlements/new`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Popular dropdown de fornecedores |
| `/finance/settlements/preview` | POST | A | Calcular preview do acerto |
| `/finance/settlements` | POST | A | Confirmar acerto |
| `/finance/settlements/:id/receipt` | GET | A | Gerar recibo PDF |

### PG-25: Detalhe do Acerto (`/finance/settlements/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/settlements/:id` | GET | R | Carregar dados do acerto |
| `/finance/settlements/:id/receipt` | GET | A | Gerar/imprimir recibo |

### PG-26: Créditos em Loja (`/finance/credits`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/credits` | GET | R | Listar créditos |
| `/finance/credits/:id` | GET | A | Ver extrato do crédito |
| `/finance/credits` | POST | A | Criar crédito manual |

### PG-27: Fluxo de Caixa (`/finance/cashflow`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/cashflow` | GET | R | Dados do fluxo de caixa |
| `/finance/cashflow/summary` | GET | R | Resumo (entradas/saídas/saldo) |

### PG-28: Despesas (`/finance/expenses`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/expenses` | GET | R | Listar despesas |
| `/finance/expenses/categories` | GET | R | Categorias para dropdown |
| `/finance/expenses` | POST | A | Registrar despesa |
| `/finance/expenses/:id` | PUT | A | Editar despesa |
| `/finance/expenses/:id` | DELETE | A | Excluir despesa |

---

## 7. Relatórios

### PG-29: Relatório de Vendas (`/reports/sales`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/sales` | GET | R/A | Carregar dados com filtros |
| `/reports/sales/export` | GET | A | Exportar PDF/Excel |
| `/brands` | GET | R | Popular filtro de marcas |
| `/categories` | GET | R | Popular filtro de categorias |

### PG-30: Relatório de Inventário (`/reports/inventory`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/inventory` | GET | R/A | Dados do relatório |
| `/reports/inventory/aging` | GET | R | Análise de aging |
| `/reports/inventory/sell-through` | GET | R | Taxa de venda |

### PG-31: Relatório de Fornecedores (`/reports/suppliers`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/suppliers` | GET | R/A | Dados do relatório |
| `/reports/suppliers/ranking` | GET | R | Ranking de fornecedores |

### PG-32: Relatório Financeiro (`/reports/finance`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/finance` | GET | R/A | Dados financeiros |
| `/reports/finance/projection` | GET | R | Projeção de fluxo de caixa |

---

## 8. Clientes

### PG-33: Lista de Clientes (`/customers`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/customers` | GET | R/A | Listar com busca |
| `/customers/:id` | DELETE | A | Excluir cliente |

### PG-34: Detalhe do Cliente (`/customers/:id`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/customers/:id` | GET | R | Dados do cliente |
| `/customers/:id/purchases` | GET | R | Histórico de compras |
| `/customers/:id/loyalty` | GET | R | Dados de fidelidade |
| `/finance/credits` | GET | R | Créditos do cliente |

### PG-35: Programa de Fidelidade (`/customers/loyalty`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/loyalty/dashboard` | GET | R | Dashboard do programa |
| `/loyalty/birthdays` | GET | R | Aniversariantes do mês |
| `/loyalty/config` | GET | R | Configuração atual |
| `/loyalty/config` | PUT | A | Atualizar configuração |

---

## 9. Promoções

### PG-36: Campanhas (`/promotions`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/promotions` | GET | R | Listar campanhas |
| `/promotions/:id` | DELETE | A | Excluir campanha |

### PG-37: Nova Campanha (`/promotions/new`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/promotions` | POST | A | Criar campanha |
| `/promotions/:id` | PUT | A | Editar campanha |
| `/promotions/:id/preview` | GET | A | Preview de itens afetados |
| `/brands` | GET | R | Popular filtro de marcas |
| `/categories` | GET | R | Popular filtro de categorias |

---

## 10. Administração

### PG-38: Usuários (`/admin/users`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/users` | GET | R | Listar usuários |
| `/admin/users` | POST | A | Criar usuário |
| `/admin/users/:id` | PUT | A | Editar usuário |
| `/admin/users/:id/deactivate` | PATCH | A | Desativar |
| `/admin/users/:id/activate` | PATCH | A | Ativar |

### PG-39: Configurações do Sistema (`/admin/settings`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/settings` | GET | R | Carregar todas as configurações |
| `/admin/settings` | PUT | A | Salvar configurações |
| `/admin/settings/:group` | GET | R | Carregar grupo específico |
| `/admin/settings/:group` | PUT | A | Salvar grupo específico |

### PG-40: Audit Log (`/admin/audit-log`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/audit-log` | GET | R/A | Listar logs com filtros |
| `/admin/audit-log/export` | GET | A | Exportar CSV |
| `/admin/users` | GET | R | Popular filtro de usuários |

### PG-41: Dados da Loja (`/admin/store`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/store` | GET | R | Carregar dados da loja |
| `/admin/store` | PUT | A | Salvar dados |
| `/admin/store/logo` | POST | A | Upload do logo |

---

## 11. Portal do Fornecedor

### PG-42: Portal - Dashboard (`/portal/dashboard`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/dashboard` | GET | R | KPIs e resumo |

### PG-43: Portal - Meus Itens (`/portal/items`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/items` | GET | R/A | Listar itens com filtros |
| `/portal/items/:id` | GET | A | Ver detalhe do item |

### PG-44: Portal - Extrato (`/portal/statements`)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/statements` | GET | R | Listar acertos |
| `/portal/statements/:id` | GET | A | Detalhe do acerto |
| `/portal/statements/:id/receipt` | GET | A | Download recibo PDF |

---

## 12. Componentes Globais

### Header (presente em todas as páginas)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/me` | GET | R | Dados do usuário logado |
| `/notifications/unread-count` | GET | P | Badge de notificações (polling 60s) |
| `/notifications` | GET | A | Ao clicar no sino |
| `/notifications/:id/read` | PATCH | A | Ao clicar na notificação |
| `/notifications/read-all` | PATCH | A | Ao clicar "marcar todas como lidas" |

### Busca Global (header)

| Endpoint | Método | Tipo | Quando |
|----------|--------|------|--------|
| `/items` | GET | A | Busca por itens |
| `/suppliers` | GET | A | Busca por fornecedores |
| `/customers` | GET | A | Busca por clientes |
| `/pos/sales` | GET | A | Busca por vendas |

---

## Resumo: Total de Endpoints por Módulo

| Módulo | Endpoints | Métodos |
|--------|-----------|---------|
| Auth | 3 | 1 POST, 2 GET |
| Dashboard | 4 | 4 GET |
| Itens | 11 | 6 GET, 2 POST, 1 PUT, 1 DELETE, 1 PATCH |
| Marcas | 4 | 2 GET, 1 POST, 1 PUT, 1 DELETE |
| Tags/Categorias | 8 | 4 GET, 2 POST, 2 PUT, 2 DELETE |
| Alertas | 2 | 2 GET |
| Consignações | 9 | 4 GET, 2 POST, 1 PUT, 1 DELETE, 2 PATCH |
| Fornecedores | 9 | 6 GET, 1 POST, 1 PUT, 1 DELETE |
| POS - Caixa | 5 | 3 GET, 2 POST |
| POS - Vendas | 6 | 3 GET, 2 POST, 1 GET |
| POS - Devoluções | 3 | 2 GET, 1 POST |
| Settlements | 6 | 3 GET, 2 POST, 1 GET |
| Créditos | 4 | 3 GET, 1 POST |
| Cashflow/Despesas | 7 | 4 GET, 1 POST, 1 PUT, 1 DELETE |
| Relatórios | 9 | 9 GET |
| Clientes | 9 | 6 GET, 1 POST, 1 PUT, 1 DELETE |
| Fidelidade | 4 | 3 GET, 1 PUT |
| Promoções | 7 | 4 GET, 1 POST, 1 PUT, 1 DELETE |
| Admin | 14 | 7 GET, 2 POST, 4 PUT, 3 PATCH |
| Portal | 7 | 1 POST, 6 GET |
| Notificações | 4 | 2 GET, 2 PATCH |
| **TOTAL** | **~135** | |
