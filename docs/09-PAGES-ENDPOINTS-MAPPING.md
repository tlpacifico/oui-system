# Oui Circular - Mapeamento Paginas x Endpoints

## Versao: 2.0 | Ultima Atualizacao: 2026-02-11

Este documento mapeia quais endpoints da API sao consumidos por cada pagina do sistema Oui Circular.

---

## Legenda

- **R** = Requisicao feita no carregamento da pagina (Read)
- **A** = Requisicao feita por acao do utilizador (Action)
- **P** = Requisicao periodica/polling

---

## 1. Autenticacao

### PG-01: Login (`/login`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/login` | POST | A | Ao clicar "Entrar" |

---

## 2. Core

### PG-02: Dashboard (`/dashboard`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/me` | GET | R | Carregar perfil do utilizador |
| `/dashboard/kpis` | GET | R/P | Carregar KPIs (auto-refresh 5min) |
| `/dashboard/sales-chart` | GET | R | Carregar grafico de vendas |
| `/dashboard/alerts` | GET | R/P | Carregar alertas pendentes |
| `/dashboard/top-sales` | GET | R | Carregar top 5 vendas |
| `/dashboard/pending-evaluations` | GET | R | Contagem de avaliacoes pendentes |
| `/notifications/unread-count` | GET | P | Badge de notificacoes (polling 1min) |

---

## 3. Inventario

### PG-03: Lista de Itens (`/inventory/items`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/items` | GET | R/A | Carregar lista, aplicar filtros, paginar |
| `/brands` | GET | R | Popular dropdown de marcas |
| `/tags` | GET | R | Popular dropdown de tags |
| `/categories` | GET | R | Popular dropdown de categorias |
| `/suppliers` | GET | R | Popular dropdown de fornecedores |
| `/items/:id` | DELETE | A | Excluir item |
| `/items/batch/labels` | POST | A | Imprimir etiquetas em lote |

### PG-04: Detalhe do Item (`/inventory/items/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/items/:id` | GET | R | Carregar dados do item |
| `/items/:id/price-history` | GET | R | Carregar historico de precos |
| `/items/:id/history` | GET | R | Carregar log de acoes |
| `/items/:id` | DELETE | A | Excluir item |
| `/items/batch/labels` | POST | A | Imprimir etiqueta |

### PG-05: Cadastro/Edicao de Item (`/inventory/items/:id/edit`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/items/:id` | GET | R | Carregar dados (modo edicao) |
| `/items` | POST | A | Salvar novo item (consignacao ou compra propria) |
| `/items/:id` | PUT | A | Atualizar item existente |
| `/items/:id/price` | PATCH | A | Alterar preco |
| `/items/:id/photos` | POST | A | Upload de fotos |
| `/items/:id/photos/:photoId` | DELETE | A | Remover foto |
| `/brands` | GET | R | Popular autocomplete de marcas |
| `/tags` | GET | R | Popular autocomplete de tags |
| `/categories` | GET | R | Popular dropdown de categorias |
| `/suppliers` | GET | R | Popular autocomplete de fornecedores (quando tipo = Consignacao) |

### PG-06: Marcas (`/inventory/brands`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/brands` | GET | R | Listar marcas |
| `/brands` | POST | A | Criar marca |
| `/brands/:id` | PUT | A | Editar marca |
| `/brands/:id` | DELETE | A | Excluir marca |

### PG-07: Tags/Categorias (`/inventory/tags`)

| Endpoint | Metodo | Tipo | Quando |
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

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/inventory/alerts` | GET | R | Listar alertas com filtro de nivel |
| `/inventory/alerts/summary` | GET | R | Contagem por nivel |
| `/items/:id/price` | PATCH | A | Reduzir preco de item |
| `/consignments/:id/return` | PATCH | A | Devolver item ao fornecedor |
| `/consignments/:id/renew` | PATCH | A | Renovar consignacao |

---

## 4. Consignacoes e Rececoes

### PG-09: Lista de Consignacoes (`/consignments`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/consignments` | GET | R/A | Listar consignacoes com filtros e paginacao |
| `/receptions` | GET | R/A | Listar rececoes (tab Rececoes) |
| `/suppliers` | GET | R | Popular filtro de fornecedores |

### PG-10: Rececao de Pecas (`/consignments/receive`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Popular autocomplete de fornecedores |
| `/suppliers` | POST | A | Cadastrar fornecedor inline (se novo) |
| `/receptions` | POST | A | Criar rececao |
| `/receptions/:id/receipt` | GET | A | Gerar recibo PDF |

### PG-10B: Avaliacoes Pendentes (`/consignments/pending-evaluations`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/receptions/pending-evaluation` | GET | R | Listar rececoes pendentes de avaliacao |

### PG-10C: Avaliar Rececao (`/consignments/:id/evaluate`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/receptions/:id` | GET | R | Carregar dados da rececao |
| `/brands` | GET | R | Popular autocomplete de marcas |
| `/tags` | GET | R | Popular autocomplete de tags |
| `/categories` | GET | R | Popular dropdown de categorias |
| `/receptions/:id/evaluate` | POST | A | Submeter avaliacao de todos os itens |
| `/receptions/:id/send-evaluation-email` | POST | A | Enviar email ao cliente |

### PG-11: Detalhe da Consignacao (`/consignments/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/consignments/:id` | GET | R | Carregar dados da consignacao |
| `/consignments/:id/evaluation-status` | GET | R | Carregar estado da avaliacao |
| `/consignments/:id/items` | POST | A | Adicionar item |
| `/consignments/:id/renew` | PATCH | A | Renovar |
| `/consignments/:id/return` | PATCH | A | Devolver itens |
| `/consignments/:id/contract` | GET | A | Gerar/imprimir contrato PDF |

### PG-12: Lista de Fornecedores (`/suppliers`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Listar fornecedores |
| `/suppliers/:id` | DELETE | A | Excluir fornecedor |

### PG-13: Detalhe do Fornecedor (`/suppliers/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers/:id` | GET | R | Dados do fornecedor |
| `/suppliers/:id/consignments` | GET | R | Consignacoes do fornecedor |
| `/suppliers/:id/receptions` | GET | R | Rececoes do fornecedor |
| `/suppliers/:id/items` | GET | R | Itens do fornecedor |
| `/suppliers/:id/settlements` | GET | R | Acertos do fornecedor |
| `/suppliers/:id/stats` | GET | R | Estatisticas |

### PG-14: Cadastro/Edicao de Fornecedor (`/suppliers/:id/edit`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers/:id` | GET | R | Carregar dados (modo edicao) |
| `/suppliers` | POST | A | Criar fornecedor |
| `/suppliers/:id` | PUT | A | Atualizar fornecedor |

### PG-15: Devolucoes ao Fornecedor (`/consignments/returns`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/inventory/alerts` | GET | R | Listar pecas com defeito e consignacoes expiradas |
| `/suppliers` | GET | R | Popular filtro de fornecedores |
| `/consignments/:id/return` | PATCH | A | Processar devolucao de itens ao fornecedor |
| `/consignments/:id/renew` | PATCH | A | Renovar consignacao (+30 dias) |
| `/items/:id` | GET | A | Ver detalhe do item |

---

## 5. Ponto de Venda (POS)

### PG-16: POS - Caixa (`/pos`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Verificar se caixa esta aberto |
| `/items` | GET | A | Buscar item por barcode/nome |
| `/customers/search` | GET | A | Buscar cliente fidelidade |
| `/pos/sales` | POST | A | Processar venda |
| `/pos/sales/:id/receipt` | GET | A | Gerar recibo para impressao |
| `/finance/credits/search` | GET | A | Buscar credito em loja (pagamento StoreCredit) |
| `/promotions/active` | GET | R | Carregar promocoes ativas |

> **Metodos de pagamento aceites:** Cash, CreditCard, DebitCard, MBWAY, StoreCredit

### PG-17: POS - Abrir Caixa (`/pos/open`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Verificar se ja tem caixa aberto |
| `/pos/register/open` | POST | A | Abrir caixa |

### PG-18: POS - Fechar Caixa (`/pos/close`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/register/current` | GET | R | Carregar resumo do caixa |
| `/pos/sales/today` | GET | R | Resumo de vendas do dia |
| `/pos/register/close` | POST | A | Fechar caixa |

### PG-19: Vendas do Dia (`/pos/sales`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales` | GET | R/A | Listar vendas com filtros |
| `/pos/sales/today` | GET | R | Totalizadores do dia |

### PG-20: Detalhe da Venda (`/pos/sales/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales/:id` | GET | R | Carregar dados da venda |
| `/pos/sales/:id/receipt` | GET | A | Gerar/imprimir recibo |
| `/pos/sales/:id/void` | POST | A | Cancelar venda |

### PG-21: Devolucoes (`/pos/returns`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/returns` | GET | R | Listar devolucoes |

### PG-22: Nova Devolucao (`/pos/returns/new`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/pos/sales` | GET | A | Buscar venda original |
| `/pos/sales/:id` | GET | A | Carregar detalhes da venda |
| `/pos/returns` | POST | A | Processar devolucao |

---

## 6. Financeiro

### PG-23: Acertos com Fornecedores (`/finance/settlements`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/settlements` | GET | R | Listar acertos |
| `/finance/settlements/pending` | GET | R | Acertos pendentes |

### PG-24: Novo Acerto (`/finance/settlements/new`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/suppliers` | GET | R | Popular dropdown de fornecedores |
| `/finance/settlements/preview` | POST | A | Calcular preview do acerto |
| `/finance/settlements` | POST | A | Confirmar acerto |
| `/finance/settlements/:id/receipt` | GET | A | Gerar recibo PDF |

### PG-25: Detalhe do Acerto (`/finance/settlements/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/settlements/:id` | GET | R | Carregar dados do acerto |
| `/finance/settlements/:id/receipt` | GET | A | Gerar/imprimir recibo |

### PG-26: Creditos em Loja (`/finance/credits`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/credits` | GET | R | Listar creditos |
| `/finance/credits/:id` | GET | A | Ver extrato do credito |
| `/finance/credits` | POST | A | Criar credito manual |

### PG-27: Fluxo de Caixa (`/finance/cashflow`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/cashflow` | GET | R | Dados do fluxo de caixa |
| `/finance/cashflow/summary` | GET | R | Resumo (entradas/saidas/saldo) |

### PG-28: Despesas (`/finance/expenses`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/finance/expenses` | GET | R | Listar despesas |
| `/finance/expenses/categories` | GET | R | Categorias para dropdown |
| `/finance/expenses` | POST | A | Registrar despesa |
| `/finance/expenses/:id` | PUT | A | Editar despesa |
| `/finance/expenses/:id` | DELETE | A | Excluir despesa |

---

## 7. Relatorios

### PG-29: Relatorio de Vendas (`/reports/sales`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/sales` | GET | R/A | Carregar dados com filtros |
| `/reports/sales/export` | GET | A | Exportar PDF/Excel |
| `/brands` | GET | R | Popular filtro de marcas |
| `/categories` | GET | R | Popular filtro de categorias |

### PG-30: Relatorio de Inventario (`/reports/inventory`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/inventory` | GET | R/A | Dados do relatorio |
| `/reports/inventory/aging` | GET | R | Analise de aging |
| `/reports/inventory/sell-through` | GET | R | Taxa de venda |

### PG-31: Relatorio de Fornecedores (`/reports/suppliers`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/suppliers` | GET | R/A | Dados do relatorio |
| `/reports/suppliers/ranking` | GET | R | Ranking de fornecedores |

### PG-32: Relatorio Financeiro (`/reports/finance`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/reports/finance` | GET | R/A | Dados financeiros |
| `/reports/finance/projection` | GET | R | Projecao de fluxo de caixa |

---

## 8. Clientes

### PG-33: Lista de Clientes (`/customers`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/customers` | GET | R/A | Listar com busca |
| `/customers/:id` | DELETE | A | Excluir cliente |

### PG-34: Detalhe do Cliente (`/customers/:id`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/customers/:id` | GET | R | Dados do cliente |
| `/customers/:id/purchases` | GET | R | Historico de compras |
| `/customers/:id/loyalty` | GET | R | Dados de fidelidade |
| `/finance/credits` | GET | R | Creditos do cliente |

### PG-35: Programa de Fidelidade (`/customers/loyalty`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/loyalty/dashboard` | GET | R | Dashboard do programa |
| `/loyalty/birthdays` | GET | R | Aniversariantes do mes |
| `/loyalty/config` | GET | R | Configuracao atual |
| `/loyalty/config` | PUT | A | Atualizar configuracao |

---

## 9. Promocoes

### PG-36: Campanhas (`/promotions`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/promotions` | GET | R | Listar campanhas |
| `/promotions/:id` | DELETE | A | Excluir campanha |

### PG-37: Nova Campanha (`/promotions/new`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/promotions` | POST | A | Criar campanha |
| `/promotions/:id` | PUT | A | Editar campanha |
| `/promotions/:id/preview` | GET | A | Preview de itens afetados |
| `/brands` | GET | R | Popular filtro de marcas |
| `/categories` | GET | R | Popular filtro de categorias |

---

## 10. Administracao

### PG-38: Utilizadores (`/admin/users`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/users` | GET | R | Listar utilizadores |
| `/admin/users` | POST | A | Criar utilizador |
| `/admin/users/:id` | PUT | A | Editar utilizador |
| `/admin/users/:id/deactivate` | PATCH | A | Desativar |
| `/admin/users/:id/activate` | PATCH | A | Ativar |

### PG-39: Configuracoes do Sistema (`/admin/settings`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/settings` | GET | R | Carregar todas as configuracoes |
| `/admin/settings` | PUT | A | Salvar configuracoes |
| `/admin/settings/:group` | GET | R | Carregar grupo especifico |
| `/admin/settings/:group` | PUT | A | Salvar grupo especifico |

### PG-40: Audit Log (`/admin/audit-log`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/audit-log` | GET | R/A | Listar logs com filtros |
| `/admin/audit-log/export` | GET | A | Exportar CSV |
| `/admin/users` | GET | R | Popular filtro de utilizadores |

### PG-41: Dados da Loja (`/admin/store`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/admin/store` | GET | R | Carregar dados da loja |
| `/admin/store` | PUT | A | Salvar dados |
| `/admin/store/logo` | POST | A | Upload do logo |

---

## 11. Portal do Fornecedor

### PG-42: Portal - Dashboard (`/portal/dashboard`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/dashboard` | GET | R | KPIs e resumo |

### PG-43: Portal - Meus Itens (`/portal/items`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/items` | GET | R/A | Listar itens com filtros |
| `/portal/items/:id` | GET | A | Ver detalhe do item |

### PG-44: Portal - Extrato (`/portal/statements`)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/portal/statements` | GET | R | Listar acertos |
| `/portal/statements/:id` | GET | A | Detalhe do acerto |
| `/portal/statements/:id/receipt` | GET | A | Download recibo PDF |

---

## 12. Componentes Globais

### Header (presente em todas as paginas)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/auth/me` | GET | R | Dados do utilizador logado |
| `/notifications/unread-count` | GET | P | Badge de notificacoes (polling 60s) |
| `/notifications` | GET | A | Ao clicar no sino |
| `/notifications/:id/read` | PATCH | A | Ao clicar na notificacao |
| `/notifications/read-all` | PATCH | A | Ao clicar "marcar todas como lidas" |

### Busca Global (header)

| Endpoint | Metodo | Tipo | Quando |
|----------|--------|------|--------|
| `/items` | GET | A | Busca por itens |
| `/suppliers` | GET | A | Busca por fornecedores |
| `/customers` | GET | A | Busca por clientes |
| `/pos/sales` | GET | A | Busca por vendas |

---

## Resumo: Total de Endpoints por Modulo

| Modulo | Endpoints | Metodos |
|--------|-----------|---------|
| Auth | 3 | 1 POST, 2 GET |
| Dashboard | 5 | 5 GET |
| Itens | 11 | 6 GET, 2 POST, 1 PUT, 1 DELETE, 1 PATCH |
| Marcas | 4 | 2 GET, 1 POST, 1 PUT, 1 DELETE |
| Tags/Categorias | 8 | 4 GET, 2 POST, 2 PUT, 2 DELETE |
| Alertas | 2 | 2 GET |
| Rececoes | 8 | 3 GET, 3 POST, 2 GET |
| Consignacoes | 8 | 4 GET, 1 POST, 1 PATCH, 1 PATCH, 1 GET |
| Fornecedores | 10 | 7 GET, 1 POST, 1 PUT, 1 DELETE |
| POS - Caixa | 5 | 3 GET, 2 POST |
| POS - Vendas | 6 | 3 GET, 2 POST, 1 GET |
| POS - Devolucoes | 3 | 2 GET, 1 POST |
| Settlements | 6 | 3 GET, 2 POST, 1 GET |
| Creditos | 4 | 3 GET, 1 POST |
| Cashflow/Despesas | 7 | 4 GET, 1 POST, 1 PUT, 1 DELETE |
| Relatorios | 9 | 9 GET |
| Clientes | 9 | 6 GET, 1 POST, 1 PUT, 1 DELETE |
| Fidelidade | 4 | 3 GET, 1 PUT |
| Promocoes | 7 | 4 GET, 1 POST, 1 PUT, 1 DELETE |
| Admin | 14 | 7 GET, 2 POST, 4 PUT, 3 PATCH |
| Portal | 7 | 1 POST, 6 GET |
| Notificacoes | 4 | 2 GET, 2 PATCH |
| **TOTAL** | **~144** | |
