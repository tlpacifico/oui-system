# SHS - Endpoints da API

## Versão: 1.0 | Última Atualização: 2026-02-11

**Base URL:** `https://api.ouisystem.com/api` (produção) | `https://localhost:5001/api` (desenvolvimento)

**Autenticação:** Firebase JWT Bearer Token em todas as rotas (exceto Auth)

**Formato:** JSON (application/json)

**Paginação padrão:** `?page=1&pageSize=20` | Resposta: `{ data: [], totalCount: number, page: number, pageSize: number }`

---

## 1. Autenticação

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/auth/login` | Validar token Firebase e retornar perfil do usuário | Não |
| POST | `/auth/refresh` | Renovar token de acesso | Sim |
| GET | `/auth/me` | Retornar dados do usuário autenticado | Sim |

---

## 2. Dashboard

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/dashboard/kpis` | KPIs resumidos: vendas hoje, receita mês, itens estoque, acertos pendentes | Cashier |
| GET | `/dashboard/sales-chart` | Dados para gráfico de vendas (7/30 dias) | Cashier |
| GET | `/dashboard/alerts` | Alertas: consignações expirando, estoque parado, caixas abertos | Cashier |
| GET | `/dashboard/top-sales` | Top 5 itens vendidos da semana | Cashier |

**Query params comuns:** `?period=today|week|month|custom&startDate=&endDate=`

---

## 3. Inventário - Itens

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/items` | Listar itens com filtros e paginação | Cashier |
| GET | `/items/:id` | Detalhe de um item | Cashier |
| POST | `/items` | Criar novo item | Manager |
| PUT | `/items/:id` | Atualizar item | Manager |
| DELETE | `/items/:id` | Soft-delete item | Manager |
| PATCH | `/items/:id/price` | Alterar preço (com regras de autorização) | Manager |
| GET | `/items/:id/price-history` | Histórico de alterações de preço | Manager |
| GET | `/items/:id/history` | Log de ações do item (criação, edição, venda) | Manager |
| POST | `/items/:id/photos` | Upload de foto(s) do item | Manager |
| DELETE | `/items/:id/photos/:photoId` | Remover foto do item | Manager |
| POST | `/items/batch/labels` | Gerar etiquetas em lote (IDs no body) | Manager |

**GET `/items` - Query params:**
```
?search=vestido          # Busca por nome/ID
&brandId=1               # Filtrar por marca
&tagIds=1,2,3            # Filtrar por tags
&size=M                  # Filtrar por tamanho
&color=Vermelho          # Filtrar por cor
&condition=Good          # Filtrar por condição
&minPrice=50             # Preço mínimo
&maxPrice=200            # Preço máximo
&status=ToSell           # Status: Evaluated, AwaitingAcceptance, ToSell, Sold, Returned
&supplierId=1            # Filtrar por fornecedor
&consignmentDateFrom=    # Data consignação de
&consignmentDateTo=      # Data consignação até
&daysInStock=30          # Mínimo de dias em estoque
&sortBy=name|price|date|daysInStock
&sortDir=asc|desc
&page=1&pageSize=20
```

---

## 4. Inventário - Marcas

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/brands` | Listar marcas | Cashier |
| GET | `/brands/:id` | Detalhe da marca | Cashier |
| POST | `/brands` | Criar marca | Manager |
| PUT | `/brands/:id` | Atualizar marca | Manager |
| DELETE | `/brands/:id` | Excluir marca (se sem itens vinculados) | Manager |

---

## 5. Inventário - Tags/Categorias

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/tags` | Listar tags | Cashier |
| POST | `/tags` | Criar tag | Manager |
| PUT | `/tags/:id` | Atualizar tag | Manager |
| DELETE | `/tags/:id` | Excluir tag | Manager |
| GET | `/categories` | Listar categorias (hierárquica) | Cashier |
| POST | `/categories` | Criar categoria | Manager |
| PUT | `/categories/:id` | Atualizar categoria | Manager |
| DELETE | `/categories/:id` | Excluir categoria | Manager |

---

## 6. Inventário - Alertas

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/inventory/alerts` | Listar itens que precisam de ação | Manager |
| GET | `/inventory/alerts/summary` | Contagem por nível (amarelo/laranja/vermelho) | Manager |

**Query params:** `?level=yellow|orange|red&page=1&pageSize=20`

---

## 7. Consignações

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/consignments` | Listar consignações com filtros | Manager |
| GET | `/consignments/:id` | Detalhe da consignação com itens | Manager |
| POST | `/consignments` | Criar nova consignação | Manager |
| PUT | `/consignments/:id` | Atualizar consignação | Manager |
| DELETE | `/consignments/:id` | Cancelar/excluir consignação | Manager |
| POST | `/consignments/:id/items` | Adicionar item à consignação | Manager |
| PATCH | `/consignments/:id/renew` | Renovar consignação (+30 dias) | Manager |
| PATCH | `/consignments/:id/return` | Devolver itens ao fornecedor | Manager |
| GET | `/consignments/:id/contract` | Gerar PDF do contrato | Manager |

**GET `/consignments` - Query params:**
```
?supplierId=1
&status=Active|Expiring|Expired|Closed
&dateFrom=&dateTo=
&page=1&pageSize=20
```

---

## 8. Fornecedores

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/suppliers` | Listar fornecedores | Manager |
| GET | `/suppliers/:id` | Detalhe do fornecedor | Manager |
| POST | `/suppliers` | Criar fornecedor | Manager |
| PUT | `/suppliers/:id` | Atualizar fornecedor | Manager |
| DELETE | `/suppliers/:id` | Soft-delete fornecedor | Manager |
| GET | `/suppliers/:id/consignments` | Consignações do fornecedor | Manager |
| GET | `/suppliers/:id/items` | Itens do fornecedor | Manager |
| GET | `/suppliers/:id/settlements` | Acertos do fornecedor | Finance |
| GET | `/suppliers/:id/stats` | Estatísticas do fornecedor | Manager |

---

## 9. Ponto de Venda (POS)

### 9.1 Caixa (Cash Register)

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| POST | `/pos/register/open` | Abrir caixa (body: openingAmount) | Cashier |
| POST | `/pos/register/close` | Fechar caixa (body: countedAmount, notes) | Cashier |
| GET | `/pos/register/current` | Status do caixa atual do usuário logado | Cashier |
| GET | `/pos/register/status` | Status de todos os caixas (para monitoramento) | Manager |
| GET | `/pos/register/history` | Histórico de aberturas/fechamentos | Manager |

### 9.2 Vendas

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| POST | `/pos/sales` | Criar/processar venda | Cashier |
| GET | `/pos/sales` | Listar vendas com filtros | Cashier |
| GET | `/pos/sales/:id` | Detalhe da venda | Cashier |
| POST | `/pos/sales/:id/void` | Cancelar/estornar venda | Manager |
| GET | `/pos/sales/:id/receipt` | Gerar recibo (PDF) | Cashier |
| GET | `/pos/sales/today` | Resumo de vendas do dia | Cashier |

**POST `/pos/sales` - Body:**
```json
{
  "items": [
    { "itemId": 123, "discount": 0 }
  ],
  "customerId": null,
  "payments": [
    { "method": "Cash", "amount": 150.00 },
    { "method": "CreditCard", "amount": 100.00 }
  ],
  "discountPercentage": 5,
  "discountReason": "Promoção quinzenal",
  "notes": ""
}
```

**GET `/pos/sales` - Query params:**
```
?dateFrom=&dateTo=
&cashierId=
&paymentMethod=Cash|CreditCard|DebitCard|Pix|StoreCredit
&page=1&pageSize=20
```

### 9.3 Devoluções

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| POST | `/pos/returns` | Processar devolução | Manager |
| GET | `/pos/returns` | Listar devoluções | Manager |
| GET | `/pos/returns/:id` | Detalhe da devolução | Manager |

**POST `/pos/returns` - Body:**
```json
{
  "saleId": 456,
  "items": [
    { "itemId": 123, "reason": "Defeito na costura" }
  ],
  "resolutionType": "StoreCredit",
  "notes": ""
}
```

---

## 10. Financeiro - Acertos (Settlements)

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/finance/settlements` | Listar acertos | Finance |
| GET | `/finance/settlements/pending` | Acertos pendentes agrupados por fornecedor | Finance |
| GET | `/finance/settlements/:id` | Detalhe do acerto | Finance |
| POST | `/finance/settlements` | Criar/processar acerto | Finance |
| GET | `/finance/settlements/:id/receipt` | Gerar recibo PDF do acerto | Finance |
| POST | `/finance/settlements/preview` | Preview do cálculo antes de confirmar | Finance |

**POST `/finance/settlements` - Body:**
```json
{
  "supplierId": 1,
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28",
  "paymentType": "Cash",
  "notes": ""
}
```

**POST `/finance/settlements/preview` - Body:**
```json
{
  "supplierId": 1,
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28",
  "paymentType": "Cash"
}
```
**Response:**
```json
{
  "supplierId": 1,
  "supplierName": "Maria Silva",
  "items": [
    {
      "itemId": 123,
      "itemName": "Vestido Floral",
      "saleDate": "2026-02-15",
      "salePrice": 100.00,
      "commissionRate": 40,
      "storeCommission": 40.00,
      "supplierPayment": 60.00
    }
  ],
  "totalSales": 500.00,
  "totalCommission": 200.00,
  "totalPayable": 300.00,
  "paymentType": "Cash"
}
```

---

## 11. Financeiro - Créditos em Loja

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/finance/credits` | Listar créditos | Finance |
| GET | `/finance/credits/:id` | Detalhe do crédito com extrato | Finance |
| POST | `/finance/credits` | Criar crédito manual | Finance |
| GET | `/finance/credits/search` | Buscar crédito por titular (para uso no POS) | Cashier |

---

## 12. Financeiro - Fluxo de Caixa e Despesas

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/finance/cashflow` | Fluxo de caixa (entradas/saídas) | Finance |
| GET | `/finance/cashflow/summary` | Resumo: total entradas, saídas, saldo | Finance |
| GET | `/finance/expenses` | Listar despesas | Finance |
| POST | `/finance/expenses` | Registrar despesa | Finance |
| PUT | `/finance/expenses/:id` | Atualizar despesa | Finance |
| DELETE | `/finance/expenses/:id` | Excluir despesa | Finance |
| GET | `/finance/expenses/categories` | Categorias de despesa | Finance |

---

## 13. Relatórios

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/reports/sales` | Relatório de vendas | Manager |
| GET | `/reports/sales/export` | Exportar relatório vendas (Excel/PDF) | Manager |
| GET | `/reports/inventory` | Relatório de inventário | Manager |
| GET | `/reports/inventory/aging` | Análise de aging do estoque | Manager |
| GET | `/reports/inventory/sell-through` | Taxa de venda por categoria/marca | Manager |
| GET | `/reports/suppliers` | Relatório de fornecedores | Manager |
| GET | `/reports/suppliers/ranking` | Ranking de fornecedores | Manager |
| GET | `/reports/finance` | Relatório financeiro consolidado | Finance |
| GET | `/reports/finance/projection` | Projeção de fluxo de caixa | Finance |

**Query params comuns para relatórios:**
```
?startDate=2026-01-01
&endDate=2026-01-31
&groupBy=day|week|month
&brandId=
&categoryId=
&supplierId=
&format=json|pdf|excel
```

---

## 14. Clientes

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/customers` | Listar clientes | Cashier |
| GET | `/customers/:id` | Detalhe do cliente | Cashier |
| POST | `/customers` | Cadastrar cliente | Cashier |
| PUT | `/customers/:id` | Atualizar cliente | Cashier |
| DELETE | `/customers/:id` | Soft-delete cliente | Manager |
| GET | `/customers/:id/purchases` | Histórico de compras | Cashier |
| GET | `/customers/:id/loyalty` | Dados de fidelidade (pontos, nível) | Cashier |
| POST | `/customers/:id/loyalty/redeem` | Resgatar pontos | Cashier |
| GET | `/customers/search` | Busca rápida (nome, CPF, telefone) | Cashier |

---

## 15. Programa de Fidelidade

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/loyalty/dashboard` | Dashboard do programa | Manager |
| GET | `/loyalty/birthdays` | Aniversariantes do mês | Manager |
| GET | `/loyalty/config` | Configuração do programa | Admin |
| PUT | `/loyalty/config` | Atualizar configuração | Admin |

---

## 16. Promoções/Campanhas

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/promotions` | Listar campanhas | Manager |
| GET | `/promotions/:id` | Detalhe da campanha | Manager |
| POST | `/promotions` | Criar campanha | Manager |
| PUT | `/promotions/:id` | Atualizar campanha | Manager |
| DELETE | `/promotions/:id` | Excluir campanha | Manager |
| GET | `/promotions/active` | Promoções ativas (para POS) | Cashier |
| GET | `/promotions/:id/preview` | Preview de itens afetados | Manager |

---

## 17. Administração

### 17.1 Usuários

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/admin/users` | Listar usuários | Admin |
| GET | `/admin/users/:id` | Detalhe do usuário | Admin |
| POST | `/admin/users` | Criar usuário (convite) | Admin |
| PUT | `/admin/users/:id` | Atualizar perfil/permissões | Admin |
| PATCH | `/admin/users/:id/deactivate` | Desativar usuário | Admin |
| PATCH | `/admin/users/:id/activate` | Reativar usuário | Admin |

### 17.2 Configurações

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/admin/settings` | Obter todas as configurações | Admin |
| PUT | `/admin/settings` | Atualizar configurações | Admin |
| GET | `/admin/settings/:group` | Obter grupo específico (consignment, pos, loyalty, alerts, fiscal) | Admin |
| PUT | `/admin/settings/:group` | Atualizar grupo específico | Admin |

### 17.3 Audit Log

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/admin/audit-log` | Listar logs de auditoria | Admin |
| GET | `/admin/audit-log/export` | Exportar logs (CSV) | Admin |

**Query params:**
```
?userId=
&action=Create|Update|Delete|Login|PriceChange|Discount|Void
&module=Inventory|Consignment|POS|Finance|Admin
&dateFrom=&dateTo=
&page=1&pageSize=50
```

### 17.4 Dados da Loja

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/admin/store` | Dados da loja | Admin |
| PUT | `/admin/store` | Atualizar dados da loja | Admin |
| POST | `/admin/store/logo` | Upload do logo | Admin |

---

## 18. Portal do Fornecedor

**Base URL:** `/portal` (autenticação separada por token do fornecedor)

| Método | Endpoint | Descrição | Auth |
|--------|----------|-----------|------|
| POST | `/portal/auth/login` | Login do fornecedor | Não |
| GET | `/portal/dashboard` | KPIs do fornecedor | Supplier |
| GET | `/portal/items` | Meus itens com filtros | Supplier |
| GET | `/portal/items/:id` | Detalhe de um item | Supplier |
| GET | `/portal/statements` | Extrato de acertos | Supplier |
| GET | `/portal/statements/:id` | Detalhe do acerto | Supplier |
| GET | `/portal/statements/:id/receipt` | Download do recibo PDF | Supplier |

---

## 19. Notificações

| Método | Endpoint | Descrição | Role Mínimo |
|--------|----------|-----------|-------------|
| GET | `/notifications` | Listar notificações do usuário | Cashier |
| PATCH | `/notifications/:id/read` | Marcar como lida | Cashier |
| PATCH | `/notifications/read-all` | Marcar todas como lidas | Cashier |
| GET | `/notifications/unread-count` | Contagem de não lidas (para badge) | Cashier |

---

## Códigos de Resposta HTTP

| Código | Significado |
|--------|-------------|
| 200 | OK - Sucesso |
| 201 | Created - Recurso criado |
| 204 | No Content - Sucesso sem corpo |
| 400 | Bad Request - Dados inválidos (validação) |
| 401 | Unauthorized - Token ausente ou inválido |
| 403 | Forbidden - Sem permissão para a ação |
| 404 | Not Found - Recurso não encontrado |
| 409 | Conflict - Conflito (ex: caixa já aberto) |
| 422 | Unprocessable Entity - Regra de negócio violada |
| 500 | Internal Server Error |

## Formato de Erro Padrão

```json
{
  "type": "ValidationError",
  "title": "Um ou mais erros de validação ocorreram",
  "status": 400,
  "errors": {
    "Name": ["O nome é obrigatório"],
    "Price": ["O preço deve ser maior que zero"]
  }
}
```
